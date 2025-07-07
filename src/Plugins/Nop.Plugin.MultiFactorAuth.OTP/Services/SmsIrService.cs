using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nop.Core;

namespace Nop.Plugin.MultiFactorAuth.OTP.Services;

/// <summary>
/// Represents SMS.ir service implementation
/// </summary>
public class SmsIrService : ISmsService
{
    #region Fields

    private readonly OTPSettings _otpSettings;
    private readonly ILogger<SmsIrService> _logger;
    private readonly HttpClient _httpClient;

    #endregion

    #region Ctor

    public SmsIrService(OTPSettings otpSettings, ILogger<SmsIrService> logger, HttpClient httpClient)
    {
        _otpSettings = otpSettings;
        _logger = logger;
        _httpClient = httpClient;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Send OTP code via SMS
    /// </summary>
    /// <param name="phoneNumber">Phone number</param>
    /// <param name="otpCode">OTP code</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
    {
        try
        {
            if (!IsConfigured())
            {
                _logger.LogWarning("SMS service is not configured");
                return false;
            }

            // Format phone number for SMS.ir API
            var formattedPhone = FormatPhoneNumber(phoneNumber);

            var requestBody = new
            {
                mobile = formattedPhone,
                templateId = _otpSettings.SmsTemplateId,
                parameters = new[]
                {
                    new { name = "Code", value = otpCode }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_otpSettings.SmsProviderBaseUrl}/v1/send/verify")
            {
                Content = content
            };

            // Use X-API-KEY header instead of token
            request.Headers.Add("X-API-KEY", _otpSettings.SmsProviderApiKey);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("SMS.ir API response: Status={StatusCode}, Content={ResponseContent}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                // Check SMS.ir response format
                if (root.TryGetProperty("status", out var statusEl))
                {
                    var status = statusEl.GetInt32();

                    if (status == 1) // Success status according to SMS.ir documentation
                    {
                        // Extract message ID and cost if available
                        if (root.TryGetProperty("data", out var dataEl))
                        {
                            if (dataEl.TryGetProperty("messageId", out var messageIdEl))
                            {
                                var messageId = messageIdEl.GetInt64();
                                _logger.LogInformation("OTP SMS sent successfully to {PhoneNumber}, MessageId: {MessageId}",
                                    formattedPhone, messageId);
                            }
                        }

                        return true;
                    }
                    else
                    {
                        // Get error message
                        var message = root.TryGetProperty("message", out var messageEl)
                            ? messageEl.GetString()
                            : "Unknown error";

                        _logger.LogWarning("SMS.ir API returned error status {Status}: {Message}", status, message);
                    }
                }
                else
                {
                    _logger.LogWarning("SMS.ir API returned unexpected response format: {Response}", responseContent);
                }
            }
            else
            {
                _logger.LogError("SMS.ir API request failed with status {StatusCode}: {Response}",
                    response.StatusCode, responseContent);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    /// <summary>
    /// Check if SMS service is configured
    /// </summary>
    /// <returns>True if configured, false otherwise</returns>
    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_otpSettings.SmsProviderApiKey) &&
               !string.IsNullOrEmpty(_otpSettings.SmsProviderBaseUrl) &&
               _otpSettings.SmsTemplateId > 0;
    }

    /// <summary>
    /// Get SMS service status
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetStatusAsync()
    {
        try
        {
            if (!IsConfigured())
                return "SMS service is not configured";

            // Test the service by checking if we can reach the API
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_otpSettings.SmsProviderBaseUrl}/v1/send/verify");
            request.Headers.Add("X-API-KEY", _otpSettings.SmsProviderApiKey);

            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                // This is expected for GET request, means API is reachable
                return "SMS service is configured and API is reachable";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return "SMS service configuration error: Invalid API key";
            }
            else
            {
                return "SMS service is configured and ready";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SMS service status");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Format phone number for SMS.ir API
    /// </summary>
    /// <param name="phoneNumber">Original phone number</param>
    /// <returns>Formatted phone number</returns>
    private string FormatPhoneNumber(string phoneNumber)
    {
        // Remove any non-digit characters
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // If it starts with 0, remove it and add +98
        if (digitsOnly.StartsWith("0"))
        {
            digitsOnly = "98" + digitsOnly.Substring(1);
        }
        // If it doesn't start with 98, add it
        else if (!digitsOnly.StartsWith("98"))
        {
            digitsOnly = "98" + digitsOnly;
        }

        return digitsOnly;
    }

    #endregion
}