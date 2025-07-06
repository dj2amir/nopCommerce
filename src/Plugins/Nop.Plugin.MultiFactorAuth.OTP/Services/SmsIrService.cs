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
    private string _token;
    private DateTime _tokenExpireTime;

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

            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to get SMS.ir token");
                return false;
            }

            var requestBody = new
            {
                Mobile = phoneNumber,
                TemplateId = _otpSettings.SmsTemplateId,
                Parameters = new[]
                {
                    new { Name = "VerificationCode", Value = otpCode }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_otpSettings.SmsProviderBaseUrl}/v1/send/verify")
            {
                Content = content
            };
            request.Headers.Add("x-sms-ir-secure-token", token);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                var isSuccessful = root.TryGetProperty("IsSuccessful", out var isSuccessEl) && isSuccessEl.GetBoolean();
                
                if (isSuccessful)
                {
                    _logger.LogInformation("OTP SMS sent successfully to {PhoneNumber}", phoneNumber);
                    return true;
                }
                else
                {
                    _logger.LogWarning("SMS.ir API returned unsuccessful response: {Response}", responseContent);
                }
            }
            else
            {
                _logger.LogError("SMS.ir API request failed with status {StatusCode}: {Response}", response.StatusCode, responseContent);
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
               !string.IsNullOrEmpty(_otpSettings.SmsProviderSecretKey) &&
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

            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return "Failed to authenticate with SMS.ir";

            return "SMS service is configured and ready";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SMS service status");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Get SMS.ir authentication token
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task<string> GetTokenAsync()
    {
        try
        {
            // Check if token is still valid
            if (!string.IsNullOrEmpty(_token) && _tokenExpireTime > DateTime.UtcNow.AddMinutes(2))
                return _token;

            var requestBody = new
            {
                UserApiKey = _otpSettings.SmsProviderApiKey,
                SecretKey = _otpSettings.SmsProviderSecretKey
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_otpSettings.SmsProviderBaseUrl}/v1/Token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("TokenKey", out var tokenEl))
                {
                    _token = tokenEl.GetString();
                    _tokenExpireTime = DateTime.UtcNow.AddMinutes(28); // SMS.ir tokens expire after 30 minutes
                    return _token;
                }
            }

            _logger.LogError("Failed to get SMS.ir token. Status: {StatusCode}, Response: {Response}", response.StatusCode, responseContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS.ir token");
            return null;
        }
    }

    #endregion
}