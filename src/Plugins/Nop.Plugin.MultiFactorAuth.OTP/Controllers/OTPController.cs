using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.OTP.Models;
using Nop.Plugin.MultiFactorAuth.OTP.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace Nop.Plugin.MultiFactorAuth.OTP.Controllers;

/// <summary>
/// Represents OTP controller
/// </summary>
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class OTPController : BasePluginController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly ISmsService _smsService;
    private readonly ILogger<OTPController> _logger;
    private readonly HttpClient _httpClient;

    #endregion

    #region Ctor

    public OTPController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        ISmsService smsService,
        ILogger<OTPController> logger,
        HttpClient httpClient)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _smsService = smsService;
        _logger = logger;
        _httpClient = httpClient;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var otpSettings = await _settingService.LoadSettingAsync<OTPSettings>(storeScope);

        var model = new ConfigurationModel
        {
            SmsProviderApiKey = otpSettings.SmsProviderApiKey,
            SmsProviderSecretKey = otpSettings.SmsProviderSecretKey,
            SmsProviderBaseUrl = otpSettings.SmsProviderBaseUrl,
            SmsTemplateId = otpSettings.SmsTemplateId,
            SenderNumber = otpSettings.SenderNumber,
            OtpCodeLength = otpSettings.OtpCodeLength,
            OtpExpirationMinutes = otpSettings.OtpExpirationMinutes,
            MaxOtpAttempts = otpSettings.MaxOtpAttempts,
            EnableForLogin = otpSettings.EnableForLogin,
            EnableForRegistration = otpSettings.EnableForRegistration
        };

        return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        // Handle test SMS
        if (Request.Form.ContainsKey("test-sms"))
        {
            return await TestSms(model);
        }

        // Handle save configuration
        if (!ModelState.IsValid)
            return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var otpSettings = await _settingService.LoadSettingAsync<OTPSettings>(storeScope);

        otpSettings.SmsProviderApiKey = model.SmsProviderApiKey;
        otpSettings.SmsProviderSecretKey = model.SmsProviderSecretKey;
        otpSettings.SmsProviderBaseUrl = model.SmsProviderBaseUrl;
        otpSettings.SmsTemplateId = model.SmsTemplateId;
        otpSettings.SenderNumber = model.SenderNumber;
        otpSettings.OtpCodeLength = model.OtpCodeLength;
        otpSettings.OtpExpirationMinutes = model.OtpExpirationMinutes;
        otpSettings.MaxOtpAttempts = model.MaxOtpAttempts;
        otpSettings.EnableForLogin = model.EnableForLogin;
        otpSettings.EnableForRegistration = model.EnableForRegistration;

        await _settingService.SaveSettingAsync(otpSettings, storeScope);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.ConfigurationSaved"));

        return await Configure();
    }

    private async Task<IActionResult> TestSms(ConfigurationModel model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.TestPhoneNumber))
            {
                model.TestResult = await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.TestSms.PhoneNumberRequired");
                _notificationService.ErrorNotification(model.TestResult);
                return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
            }

            // Validate phone number format
            var phoneRegex = new System.Text.RegularExpressions.Regex(@"^09\d{9}$");
            if (!phoneRegex.IsMatch(model.TestPhoneNumber))
            {
                model.TestResult = "شماره تلفن باید با فرمت صحیح (09xxxxxxxxx) وارد شود";
                _notificationService.ErrorNotification(model.TestResult);
                return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
            }

            // Validate required fields
            if (string.IsNullOrEmpty(model.SmsProviderApiKey))
            {
                model.TestResult = "کلید API سرویس پیامک الزامی است";
                _notificationService.ErrorNotification(model.TestResult);
                return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
            }

            if (string.IsNullOrEmpty(model.SmsProviderBaseUrl))
            {
                model.TestResult = "آدرس پایه سرویس پیامک الزامی است";
                _notificationService.ErrorNotification(model.TestResult);
                return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
            }

            if (model.SmsTemplateId <= 0)
            {
                model.TestResult = "شناسه قالب پیامک الزامی است";
                _notificationService.ErrorNotification(model.TestResult);
                return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
            }

            // Test sending SMS directly using SMS.ir API
            var result = await SendTestSmsAsync(model);

            model.TestResult = result
                ? await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.TestSms.Success")
                : await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.TestSms.Failed");

            if (result)
            {
                _notificationService.SuccessNotification(model.TestResult);
            }
            else
            {
                _notificationService.ErrorNotification(model.TestResult);
            }
        }
        catch (Exception ex)
        {
            model.TestResult = string.Format(await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.TestSms.Error"), ex.Message);
            _notificationService.ErrorNotification(model.TestResult);
            _logger.LogError(ex, "Error in test SMS");
        }

        return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
    }

    private async Task<bool> SendTestSmsAsync(ConfigurationModel model)
    {
        try
        {
            // Format phone number for SMS.ir API
            var formattedPhone = FormatPhoneNumber(model.TestPhoneNumber);

            // Generate test OTP code
            var testCode = "123456";

            var requestBody = new
            {
                mobile = formattedPhone,
                templateId = model.SmsTemplateId,
                parameters = new[]
                {
                    new { name = "Code", value = testCode }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{model.SmsProviderBaseUrl}/v1/send/verify")
            {
                Content = content
            };

            // Use X-API-KEY header for SMS.ir authentication
            request.Headers.Add("X-API-KEY", model.SmsProviderApiKey);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Test SMS API response: Status={StatusCode}, Content={ResponseContent}", 
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
                        // Extract message ID if available
                        if (root.TryGetProperty("data", out var dataEl))
                        {
                            if (dataEl.TryGetProperty("messageId", out var messageIdEl))
                            {
                                var messageId = messageIdEl.GetInt64();
                                _logger.LogInformation("Test SMS sent successfully to {PhoneNumber}, MessageId: {MessageId}", 
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
            _logger.LogError(ex, "Error sending test SMS");
            return false;
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