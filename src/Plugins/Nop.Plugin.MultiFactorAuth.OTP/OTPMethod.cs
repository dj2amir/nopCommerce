using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.OTP.Components;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Microsoft.Extensions.Logging;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.MultiFactorAuth.OTP;

/// <summary>
/// Represents method for the multi-factor authentication with OTP
/// </summary>
public class OTPMethod : BasePlugin, IMultiFactorAuthenticationMethod, IAdminMenuPlugin
{
    #region Fields

    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly ILogger<OTPMethod> _logger;

    #endregion

    #region Ctor

    public OTPMethod(IActionContextAccessor actionContextAccessor,
        ILocalizationService localizationService,
        ISettingService settingService,
        IStoreContext storeContext,
        IUrlHelperFactory urlHelperFactory,
        ILogger<OTPMethod> logger)
    {
        _actionContextAccessor = actionContextAccessor;
        _localizationService = localizationService;
        _settingService = settingService;
        _storeContext = storeContext;
        _urlHelperFactory = urlHelperFactory;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        // روش ساده‌تر و مطمئن‌تر
        return "/Admin/OTP/Configure";
    }

    /// <summary>
    /// Manage sitemap. You can use "SystemName" of menu items to manage existing sitemap or add a new menu item.
    /// </summary>
    /// <param name="rootNode">Root node of the sitemap.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        // ایجاد یا پیدا کردن منوی Configuration
        var configurationNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Configuration");
        if (configurationNode == null)
        {
            configurationNode = new AdminMenuItem()
            {
                SystemName = "Configuration",
                Title = await _localizationService.GetResourceAsync("Admin.Configuration"),
                Visible = true,
                IconClass = "fa-gear"
            };
            rootNode.ChildNodes.Add(configurationNode);
        }

        // پیدا کردن یا ایجاد منوی Authentication
        var authenticationNode = configurationNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Authentication");
        if (authenticationNode == null)
        {
            authenticationNode = new AdminMenuItem()
            {
                SystemName = "Authentication",
                Title = await _localizationService.GetResourceAsync("Admin.Configuration.Authentication"),
                Visible = true,
                IconClass = "fa-shield"
            };
            configurationNode.ChildNodes.Add(authenticationNode);
        }

        // ایجاد منوی OTP Configuration
        var otpConfigNode = new AdminMenuItem()
        {
            SystemName = "OTPConfig",
            Title = "تنظیمات OTP",
            Url = "/Admin/OTP/Configure",
            Visible = true,
            IconClass = "fa-mobile-alt"
        };

        authenticationNode.ChildNodes.Add(otpConfigNode);
    }

    /// <summary>
    /// Gets a type of a view component for displaying plugin in public store
    /// </summary>
    /// <returns>View component type</returns>
    public Type GetPublicViewComponent()
    {
        return typeof(OTPAuthenticationViewComponent);
    }

    /// <summary>
    /// Gets a type of a view component for displaying verification page
    /// </summary>
    /// <returns>View component type</returns>
    public Type GetVerificationViewComponent()
    {
        return typeof(OTPVerificationViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        _logger.LogInformation("OTP Plugin: Installing...");
        
        // Settings
        await _settingService.SaveSettingAsync(new OTPSettings
        {
            SmsProviderBaseUrl = "https://api.sms.ir",
            OtpCodeLength = OTPDefaults.DefaultOtpCodeLength,
            OtpExpirationMinutes = OTPDefaults.DefaultOtpExpirationMinutes,
            MaxOtpAttempts = OTPDefaults.DefaultMaxOtpAttempts,
            EnableForLogin = true,
            EnableForRegistration = false
        });

        // Locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Main title
            ["Plugins.MultiFactorAuth.OTP.Configure"] = "پیکربندی OTP",
            ["Plugins.MultiFactorAuth.OTP.ConfigurationTitle"] = "تنظیمات OTP",
            
            // Configuration sections
            ["Plugins.MultiFactorAuth.OTP.Configuration.SmsProviderSettings"] = "تنظیمات ارائه‌دهنده پیامک",
            ["Plugins.MultiFactorAuth.OTP.Configuration.OtpSettings"] = "تنظیمات کد OTP",
            ["Plugins.MultiFactorAuth.OTP.Configuration.FeatureSettings"] = "تنظیمات ویژگی‌ها",
            ["Plugins.MultiFactorAuth.OTP.Configuration.TestSettings"] = "تست ارسال پیامک",
            
            // Admin configuration
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderApiKey"] = "کلید API سرویس پیامک",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderApiKey.Hint"] = "کلید API دریافت شده از سرویس پیامک را وارد کنید",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderSecretKey"] = "کلید مخفی سرویس پیامک",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderSecretKey.Hint"] = "کلید مخفی دریافت شده از سرویس پیامک را وارد کنید",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderBaseUrl"] = "آدرس پایه سرویس پیامک",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderBaseUrl.Hint"] = "آدرس پایه API سرویس پیامک (مثال: https://api.sms.ir)",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsTemplateId"] = "شناسه قالب پیامک",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsTemplateId.Hint"] = "شناسه قالب پیامک که در سرویس پیامک تعریف شده است",
            ["Plugins.MultiFactorAuth.OTP.Fields.SenderNumber"] = "شماره فرستنده",
            ["Plugins.MultiFactorAuth.OTP.Fields.SenderNumber.Hint"] = "شماره فرستنده پیامک",
            ["Plugins.MultiFactorAuth.OTP.Fields.OtpCodeLength"] = "طول کد OTP",
            ["Plugins.MultiFactorAuth.OTP.Fields.OtpCodeLength.Hint"] = "تعداد رقم کد OTP (بین 4 تا 8 رقم)",
            ["Plugins.MultiFactorAuth.OTP.Fields.OtpExpirationMinutes"] = "مدت انقضای کد OTP (دقیقه)",
            ["Plugins.MultiFactorAuth.OTP.Fields.OtpExpirationMinutes.Hint"] = "مدت زمان اعتبار کد OTP به دقیقه",
            ["Plugins.MultiFactorAuth.OTP.Fields.MaxOtpAttempts"] = "حداکثر تلاش برای OTP",
            ["Plugins.MultiFactorAuth.OTP.Fields.MaxOtpAttempts.Hint"] = "حداکثر تعداد تلاش برای وارد کردن کد OTP",
            ["Plugins.MultiFactorAuth.OTP.Fields.EnableForLogin"] = "فعال برای ورود",
            ["Plugins.MultiFactorAuth.OTP.Fields.EnableForLogin.Hint"] = "فعال کردن تایید دو مرحله‌ای برای ورود",
            ["Plugins.MultiFactorAuth.OTP.Fields.EnableForRegistration"] = "فعال برای ثبت‌نام",
            ["Plugins.MultiFactorAuth.OTP.Fields.EnableForRegistration.Hint"] = "فعال کردن تایید شماره تلفن برای ثبت‌نام",

            // Test SMS fields
            ["Plugins.MultiFactorAuth.OTP.Fields.TestPhoneNumber"] = "شماره تلفن تست",
            ["Plugins.MultiFactorAuth.OTP.Fields.TestPhoneNumber.Hint"] = "شماره تلفن برای تست ارسال پیامک",
            ["Plugins.MultiFactorAuth.OTP.TestSms"] = "تست پیامک",
            ["Plugins.MultiFactorAuth.OTP.TestResult"] = "نتیجه تست",

            // Customer fields
            ["Plugins.MultiFactorAuth.OTP.Fields.PhoneNumber"] = "شماره تلفن همراه",
            ["Plugins.MultiFactorAuth.OTP.Fields.PhoneNumber.Hint"] = "شماره تلفن همراه برای دریافت کد OTP",
            ["Plugins.MultiFactorAuth.OTP.Fields.OtpCode"] = "کد تایید",
            ["Plugins.MultiFactorAuth.OTP.Fields.OtpCode.Hint"] = "کد تایید دریافت شده از طریق پیامک",
            ["Plugins.MultiFactorAuth.OTP.Fields.BackupCode"] = "کد پشتیبان",
            ["Plugins.MultiFactorAuth.OTP.Fields.BackupCode.Hint"] = "کد پشتیبان در صورت عدم دسترسی به پیامک",
            ["Plugins.MultiFactorAuth.OTP.Fields.IsPhoneNumberVerified"] = "شماره تلفن تایید شده",
            ["Plugins.MultiFactorAuth.OTP.Fields.IsEnabled"] = "فعال",

            // Instructions
            ["Plugins.MultiFactorAuth.OTP.Instructions"] = "برای فعال کردن تایید دو مرحله‌ای، شماره تلفن همراه خود را وارد کنید و آن را تایید کنید.",
            ["Plugins.MultiFactorAuth.OTP.VerificationInstructions"] = "کد تایید دریافت شده از طریق پیامک را وارد کنید.",
            ["Plugins.MultiFactorAuth.OTP.MultiFactorAuthenticationMethodDescription"] = "تایید دو مرحله‌ای از طریق پیامک - کد تایید به شماره تلفن همراه شما ارسال می‌شود",

            // Messages
            ["Plugins.MultiFactorAuth.OTP.OtpSent"] = "کد تایید به شماره تلفن شما ارسال شد",
            ["Plugins.MultiFactorAuth.OTP.OtpSendFailed"] = "خطا در ارسال کد تایید",
            ["Plugins.MultiFactorAuth.OTP.OtpVerified"] = "کد تایید صحیح است",
            ["Plugins.MultiFactorAuth.OTP.OtpInvalid"] = "کد تایید نامعتبر است",
            ["Plugins.MultiFactorAuth.OTP.OtpExpired"] = "کد تایید منقضی شده است",
            ["Plugins.MultiFactorAuth.OTP.MaxAttemptsReached"] = "حداکثر تعداد تلاش تجاوز شد",
            ["Plugins.MultiFactorAuth.OTP.PhoneNumberRequired"] = "شماره تلفن همراه الزامی است",
            ["Plugins.MultiFactorAuth.OTP.PhoneNumberInvalid"] = "شماره تلفن همراه نامعتبر است",
            ["Plugins.MultiFactorAuth.OTP.BackupCodeUsed"] = "کد پشتیبان با موفقیت استفاده شد",
            ["Plugins.MultiFactorAuth.OTP.BackupCodeInvalid"] = "کد پشتیبان نامعتبر است",
            ["Plugins.MultiFactorAuth.OTP.ConfigurationSaved"] = "تنظیمات OTP با موفقیت ذخیره شد",
            ["Plugins.MultiFactorAuth.OTP.PhoneNumberVerified"] = "شماره تلفن با موفقیت تایید شد",

            // Test SMS
            ["Plugins.MultiFactorAuth.OTP.TestSms.PhoneNumberRequired"] = "شماره تلفن برای تست الزامی است",
            ["Plugins.MultiFactorAuth.OTP.TestSms.Success"] = "پیامک تست با موفقیت ارسال شد",
            ["Plugins.MultiFactorAuth.OTP.TestSms.Failed"] = "خطا در ارسال پیامک تست",
            ["Plugins.MultiFactorAuth.OTP.TestSms.Error"] = "خطا در ارسال پیامک تست: {0}",

            // Validation
            ["Plugins.MultiFactorAuth.OTP.Fields.OtpCode.Required"] = "کد تایید الزامی است",
            ["Plugins.MultiFactorAuth.OTP.Fields.PhoneNumber.Required"] = "شماره تلفن همراه الزامی است",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderApiKey.Required"] = "کلید API سرویس پیامک الزامی است",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderSecretKey.Required"] = "کلید مخفی سرویس پیامک الزامی است",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsProviderBaseUrl.Required"] = "آدرس پایه سرویس پیامک الزامی است",
            ["Plugins.MultiFactorAuth.OTP.Fields.SmsTemplateId.Required"] = "شناسه قالب پیامک الزامی است",
            
            // Customer UI additional resources
            ["Plugins.MultiFactorAuth.OTP.Enabled"] = "تایید دو مرحله‌ای فعال است",
            ["Plugins.MultiFactorAuth.OTP.PhoneNumberVerified"] = "شماره تلفن تایید شده",
            ["Plugins.MultiFactorAuth.OTP.PhoneNumberNotVerified"] = "شماره تلفن تایید نشده",
            ["Plugins.MultiFactorAuth.OTP.SendCode"] = "ارسال کد تایید",
            ["Plugins.MultiFactorAuth.OTP.VerifyPhone"] = "تایید شماره تلفن",
            ["Plugins.MultiFactorAuth.OTP.Enable"] = "فعال کردن تایید دو مرحله‌ای",
            ["Plugins.MultiFactorAuth.OTP.Disable"] = "غیرفعال کردن تایید دو مرحله‌ای",
            ["Plugins.MultiFactorAuth.OTP.GenerateBackupCodes"] = "تولید کدهای پشتیبان",
            ["Plugins.MultiFactorAuth.OTP.ShowBackupCodes"] = "نمایش کدهای پشتیبان",
            ["Plugins.MultiFactorAuth.OTP.BackupCodes"] = "کدهای پشتیبان",
            ["Plugins.MultiFactorAuth.OTP.BackupCodes.Warning"] = "این کدها را در مکان امنی نگهداری کنید. هر کد تنها یک بار قابل استفاده است."
        });

        _logger.LogInformation("OTP Plugin: Installation completed");
        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        _logger.LogInformation("OTP Plugin: Uninstalling...");
        
        // Settings
        await _settingService.DeleteSettingAsync<OTPSettings>();

        // Locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.MultiFactorAuth.OTP");

        _logger.LogInformation("OTP Plugin: Uninstallation completed");
        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a multi-factor authentication method description that will be displayed on customer info pages in the public store
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetDescriptionAsync()
    {
        return await _localizationService
            .GetResourceAsync("Plugins.MultiFactorAuth.OTP.MultiFactorAuthenticationMethodDescription");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a multi-factor authentication type
    /// </summary>
    public MultiFactorAuthenticationType Type => MultiFactorAuthenticationType.SMSVerification;

    #endregion
}