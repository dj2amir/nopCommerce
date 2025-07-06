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

    #endregion

    #region Ctor

    public OTPController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        ISmsService smsService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _smsService = smsService;
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

            // Temporarily create settings for testing
            var testSettings = new OTPSettings
            {
                SmsProviderApiKey = model.SmsProviderApiKey,
                SmsProviderSecretKey = model.SmsProviderSecretKey,
                SmsProviderBaseUrl = model.SmsProviderBaseUrl,
                SmsTemplateId = model.SmsTemplateId,
                SenderNumber = model.SenderNumber
            };

            // Generate test OTP code
            var testCode = "123456";
            
            // Test sending SMS
            var result = await _smsService.SendOtpAsync(model.TestPhoneNumber, testCode);

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
        }

        return View("~/Plugins/MultiFactorAuth.OTP/Views/Configure.cshtml", model);
    }

    #endregion
}