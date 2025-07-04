using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using InjaKalaIR.Plugin.MobileValidation.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation
{
    public class MobileValidationPlugin : BasePlugin, IPlugin,
        //IAdminMenuPlugin,
        IWidgetPlugin
    {
        private readonly ILocalizationService localizationService;

        public bool HideInWidgetList => false;

        public ISettingService SettingService { get; }

        public MobileValidationPlugin(
            IWebHelper webHelper,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
           

            SettingService = settingService;
            this.localizationService = localizationService;
        }

        public override Task InstallAsync()
        {
            var model = new MobileValidationSettings()
            {
                MobileValidationNewPass = "رمز عبور جدید: {0}",
                MobileValidationActiveCode = "کد فعالسازی جدید:{0}"
            };
            this.SettingService.SaveSettingAsync(model).Wait();
            //EngineContext.Current.Resolve<ISettingService>().SetSetting<string>("MobileValidationSms","کد فعالسازی {0}");
            //EngineContext.Current.Resolve<ISettingService>().SetSetting<string>("MobileValidationNewPass", "رمز عبور جدید: {0}");
            //EngineContext.Current.Resolve<ISettingService>().SetSetting<string>("MobileValidationActiveCode", "کد فعالسازی جدید:{0}");
            //فعال کردن کلمه عبور
            EngineContext.Current.Resolve<ISettingService>().SetSettingAsync("customersettings.usernamesenabled", true).Wait();

            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.Account.PasswordRecovery.Mobile", "موبایل", null).Wait(); ;
            localizationService.AddOrUpdateLocaleResourceAsync("mobileValidation.fields.codeValidation", "کد فعالسازی", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.Account.PasswordRecovery.Tooltip", "لطفا موبایل خود را وارد کنید. کد فعالسازی به موبایل شما ارسال خواهد شد.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.Account.PasswordRecovery.SmsHasBeenSent", "پیام ارسال شد.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.Account.PasswordRecovery.EmailNotFound", "چنین شماره موبایلی پیدا نشد.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.Fields.Heading", "تایید موبایل", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.WebSiteHashCode", "Private Password", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.WebSiteHashCode.Hint", "You are the author of the mail.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsUsername", "نام کاربری پیامک", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsUsername.Hint", "نام کاربری وب سرویس پیامک", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.LineNumber", "شماره خط ارسال", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.LineNumber.Hint", "شماره خط پیامک که از ارائه دهنده سامانه پیامک دریافت کردید را در این قسمت وارد کنید.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsPassword", "رمز وب سرویس", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsPassword.Hint", "رمز وب سرویس که در سامانه پیامک وارد کردید را در این قسمت وارد کنید.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsRemaining", "اعتبار باقیمانده پیامک", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsRemaining.Hint", "Sms Remaining Credits of sms service.It is advised to follow the sms company", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsIsActive", "فعال", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.IsValidation", "اعتبار سنجی موبایل", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.IsValidation.Hint", "این ویژگی باعث می شود بعد از ثبت نام کاربر پیامکی ارسال شده و کد در خواستی را در سامانه وارد کند. پس از وارد کردن کاربر فعال می شود.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.Plugin.SMS.SmsIsActive.Hint", "Is the SMS service active?", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.account.passwordrecovery.smsnotfound", "اکانت یافت نشد", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("account.fields.username", "نام کاربری (تلفن همراه)", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("account.login.fields.username", "نام کاربری (تلفن همراه)", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsFirstNameOptional", "ورود نام و نام خانوادگی دلخواه شود", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsFirstNameOptional.Hint", "در صفحه ثبت نام اگر تمایل دارید نام و نام خانوادگی به صورت دلخواه توسط مشتری وارد شود این گزینه را فعال کنید.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsEmailOptional", "ورود ایمیل دلخواه شود", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("Account.PasswordRecovery.RecoverButtonConfirm", "تایید", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.MobileValidationActiveCode", "متن کد فعالسازی", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.MobileValidationActiveCode.Hint", "متن کد فعال سازی که هنگام ثبت نام یا ورود ارسال می شود را در این قسمت تنظیم نمایید.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.MobileValidationNewPass", "متن بازیابی متن", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.MobileValidationNewPass.Hint", "متن بازیابی رمز در قسمت فراموشی رمز را در این قسمت تنظیم نمایید.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.WebServiceSupport", "وب سرویس پیامک", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.WebServiceSupport.Hint", "وب سرویس سامانه پیامک که تهیه کردید و قصد استفاده دارید را انتخاب نمایید.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsIsUseCustomLogin", "صفحه لاگین سفارشی (بررسی اعداد فارسی)", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.SmsIsUseCustomLogin.Hint", "در این قسمت در صورتی که اعداد با کارکتر فارسی در صفحه لاگین وارد شود تبدیل به انگلیسی می شود این کار به دلیل اختلاف در کیبورد های مختلف می باشد.", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.EnableOtp", "Enable OTP", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.EnableOtp.Hint", "فعالسازی رمز یکبار استفاده", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.MenuItem", "ورود با موبایل", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.OtpText", "شماره موبایل یا ایمیل خود را وارد نماييد", null).Wait();
            localizationService.AddOrUpdateLocaleResourceAsync("InjaKala.MobileValidation.OtpText.hint", "جهت ورود یا عضویت شماره موبایل یا ایمیل خود را وارد نماييد", null).Wait();
            return base.InstallAsync();
        }

        public override Task UninstallAsync()
        {
            localizationService.DeleteLocaleResourceAsync("InjaKala.Account.PasswordRecovery.Mobile").Wait();
            localizationService.DeleteLocaleResourceAsync("InjaKala.Account.PasswordRecovery.Tooltip").Wait();
            localizationService.DeleteLocaleResourceAsync("InjaKala.Account.PasswordRecovery.SmsHasBeenSent").Wait();
            localizationService.DeleteLocaleResourceAsync("InjaKala.Account.PasswordRecovery.EmailNotFound").Wait();
            localizationService.DeleteLocaleResourceAsync("InjaKala.MobileValidation.Fields.Heading").Wait();
            return base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
          

            return webHelper.GetStoreLocation() + "Admin/InjaKalaIRSmsValidationConfigure/Configure";
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            string viewComponentName = "MobileValidation";
            if (widgetZone.Contains("account_navigation"))
            {
                viewComponentName = "WidgetsMobileMyAccountPublicInfo";
            }
            return viewComponentName;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>() { "main_column_after", "account_navigation_after" });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone.Contains("account_navigation"))
            {
                return typeof(WidgetsMobileMyAccountPublicInfoViewComponent);
            }
            return typeof(MobileValidationViewComponent);
        }
    }
}