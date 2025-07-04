using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using InjaKalaIR.Plugin.MobileValidation.Models;
using InjaKalaIR.Plugin.MobileValidation.Service;
using InjaKalaIR.Plugin.MobileValidation.SmsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InjaKalaIR.Plugin.MobileValidation;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.ScheduleTasks;

namespace InjaKalaIR.Plugin.MobileValidation.Controllers
{
    public class InjaKalaIRSmsValidationConfigure : BaseAdminController
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _ischeduleTaskService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        public InjaKalaIRSmsValidationConfigure(
            IStoreContext storeContext,
            ISettingService settingService,
            IScheduleTaskService ischeduleTaskService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            this._storeContext = storeContext;
            this._settingService = settingService;
            this._ischeduleTaskService = ischeduleTaskService;
            this._localizationService = localizationService;
            this._notificationService = notificationService;
        }

        [HttpGet]
        [AuthorizeAdmin]
        [Area("Admin")]
        public ActionResult Configure()
        {

            int activeStoreScopeConfiguration = _storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            MobileValidationSettings MobileValidationSettings = this._settingService.LoadSettingAsync<MobileValidationSettings>(activeStoreScopeConfiguration).Result;
            var configurationModel = new ConfigurationModel();
            configurationModel.WebSiteHashCode = MobileValidationSettings.WebSiteHashPassword;
            configurationModel.SmsUsername = MobileValidationSettings.SmsUsername;
            configurationModel.SmsPassword = MobileValidationSettings.SmsPassword;
            configurationModel.LineNumber = MobileValidationSettings.SmsLineNumber;
            configurationModel.SmsRemaining = MobileValidationSettings.SmsRemaining;
            //if (DateTime.Now.Month == 12 && DateTime.Now.Day > 17 && DateTime.Now.Day < 20) 
            {
                configurationModel.SmsIsActive = MobileValidationSettings.SmsIsActive;
                configurationModel.SmsIsSendValidation = MobileValidationSettings.SmsIsSendValidation;
                configurationModel.SmsFirstNameOptional = MobileValidationSettings.SmsFirstNameOptional;
                configurationModel.SmsEmailOptional = MobileValidationSettings.SmsEmailOptional;
                configurationModel.MobileValidationActiveCode = MobileValidationSettings.MobileValidationActiveCode;
            }
            //else
            //{
            //    return Content("");
            //}
            configurationModel.SmsIsActive = MobileValidationSettings.SmsIsActive;
            configurationModel.SmsIsSendValidation = MobileValidationSettings.SmsIsSendValidation;
            configurationModel.SmsFirstNameOptional = MobileValidationSettings.SmsFirstNameOptional;
            configurationModel.SmsEmailOptional = MobileValidationSettings.SmsEmailOptional;
            configurationModel.MobileValidationActiveCode = MobileValidationSettings.MobileValidationActiveCode;
            configurationModel.MobileValidationNewPass = MobileValidationSettings.MobileValidationNewPass;
            configurationModel.SmsIsUseCustomLogin = MobileValidationSettings.SmsIsUseCustomLogin;
            configurationModel.EnableOtp = MobileValidationSettings.EnableOtp;

            configurationModel.SmsSubject = MobileValidationSettings.SmsSubject;
            configurationModel.ActiveStoreScopeConfiguration = activeStoreScopeConfiguration;

            configurationModel.InjaKala = "تولید و توسعه توسط ناپ فارسی http://InjaKala.ir";

            if (activeStoreScopeConfiguration > 0)
            {
                configurationModel.WebSiteHashCode_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.WebSiteHashPassword, activeStoreScopeConfiguration).Result;
                configurationModel.SmsPassword_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsPassword, activeStoreScopeConfiguration).Result;
                configurationModel.LineNumber_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsLineNumber, activeStoreScopeConfiguration).Result;
                configurationModel.SmsRemaining_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, int?>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsRemaining, activeStoreScopeConfiguration).Result;
                configurationModel.SmsUsername_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsUsername, activeStoreScopeConfiguration).Result;
                configurationModel.SmsIsActive_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsIsActive, activeStoreScopeConfiguration).Result;
                configurationModel.EnableOtp_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.EnableOtp, activeStoreScopeConfiguration).Result;
                configurationModel.SmsFirstNameOptional = this._settingService.SettingExistsAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsFirstNameOptional, activeStoreScopeConfiguration).Result;
                configurationModel.SmsEmailOptional = this._settingService.SettingExistsAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsEmailOptional, activeStoreScopeConfiguration).Result;
                configurationModel.SmsSubject_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsSubject, activeStoreScopeConfiguration).Result;
                configurationModel.MobileValidationActiveCode_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.MobileValidationActiveCode, activeStoreScopeConfiguration).Result;
                configurationModel.MobileValidationNewPass_OverrideForStore = this._settingService.SettingExistsAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.MobileValidationNewPass, activeStoreScopeConfiguration).Result;
            }

            ViewBag.webServiceName = "وب سرویس در حال استفاده: " + CurrentSmsService.SmsService.Name;
            //notificationService.SuccessNotification("خورسندیم که از این پلاگین استفاده می کنید. جهت اطلاعات بیشتر به سایت ناپ فارسی مراجعه شود.www.InjaKala.ir", true);

            var assemblyName = "InjaKalaIR.Plugin.MobileValidation";

            var asm = Assembly.Load(assemblyName);
            
            var type = typeof(ISmsService);
            var types = asm.GetTypes()
                .Where(p => type.IsAssignableFrom(p) && p.IsClass);

            IList<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (var item in types)
            {
                var v = ((ISmsService)Activator.CreateInstance(item));
                selectListItems.Add(new SelectListItem(v.Name, v.Id.ToString(), v.Id == MobileValidationSettings.WebServiceSupport));
            }

            
            ViewBag.webServices = selectListItems;
            return base.View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/Configuration.cshtml", configurationModel);
        }

        [Area("Admin")]
        [AuthorizeAdmin]
        [ActionName("Configure"), FormValueRequired(new string[]
        {
            "save"
        }), HttpPost]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!base.ModelState.IsValid)
            {
                return this.Configure();
            }
            if (!string.IsNullOrWhiteSpace(model.Error))
            {
                model.Error = "";
            }
            if (model.InjaKala != "تولید و توسعه توسط ناپ فارسی http://InjaKala.ir")
            {
                return this.Configure();
            }

            EngineContext.Current.Resolve<IStaticCacheManager>().RemoveByPrefixAsync("SMS_WebService").Wait();

            int activeStoreScopeConfiguration = this._storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            MobileValidationSettings MobileValidationSettings = this._settingService.LoadSettingAsync<MobileValidationSettings>(activeStoreScopeConfiguration).Result;
            if (!string.IsNullOrEmpty(RouteProvider.Routes(Request)))
            {
                EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Error, "Error Code : 1054", null, null).Wait();
                return base.Content("OK");
            }
            if (model.SmsIsActive)
            {
                using (IEnumerator<ScheduleTask> enumerator = this._ischeduleTaskService.GetAllTasksAsync(true).Result.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ScheduleTask current = enumerator.Current;
                        if (current.Id == MobileValidationSettings.TaskId && !current.Enabled)
                        {
                            current.Enabled = (true);
                            this._ischeduleTaskService.UpdateTaskAsync(current).Wait();
                        }
                    }
                }
            }
            else
            {
                foreach (ScheduleTask current2 in this._ischeduleTaskService.GetAllTasksAsync(true).Result)
                {
                    if (current2.Id == MobileValidationSettings.TaskId && current2.Enabled)
                    {
                        current2.Enabled = (false);
                        this._ischeduleTaskService.UpdateTaskAsync(current2).Wait();
                    }
                }
            }

            MobileValidationSettings.WebSiteHashPassword = model.WebSiteHashCode;
            MobileValidationSettings.SmsPassword = model.SmsPassword;
            MobileValidationSettings.SmsLineNumber = model.LineNumber;
            MobileValidationSettings.SmsRemaining = model.SmsRemaining;
            MobileValidationSettings.SmsUsername = model.SmsUsername;
            MobileValidationSettings.SmsIsActive = model.SmsIsActive;
            MobileValidationSettings.SmsIsSendValidation = model.SmsIsSendValidation;
            MobileValidationSettings.SmsFirstNameOptional = model.SmsFirstNameOptional;
            MobileValidationSettings.SmsEmailOptional = model.SmsEmailOptional;
            MobileValidationSettings.SmsSubject = model.SmsSubject;
            MobileValidationSettings.MobileValidationActiveCode = model.MobileValidationActiveCode;
            MobileValidationSettings.MobileValidationNewPass = model.MobileValidationNewPass;
            MobileValidationSettings.WebServiceSupport = model.WebServiceSupport;
            MobileValidationSettings.SmsIsUseCustomLogin = model.SmsIsUseCustomLogin;
            MobileValidationSettings.EnableOtp = model.EnableOtp;

            if (model.ActiveStoreScopeConfiguration > 0)
            {
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.WebSiteHashPassword, model.WebSiteHashCode_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsPassword, model.SmsPassword_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsLineNumber, model.LineNumber_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, int?>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsRemaining, model.SmsRemaining_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsUsername, model.SmsUsername_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsIsActive, model.SmsIsActive_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.EnableOtp, model.EnableOtp_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsIsSendValidation, model.SmsIsSendValidation_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsFirstNameOptional, model.SmsFirstNameOptional_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsEmailOptional, model.SmsEmailOptional_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsSubject, model.SmsSubject_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.AbandonedContent, true, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, int>(MobileValidationSettings, (MobileValidationSettings x) => x.AbandonedDay, true, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, int>(MobileValidationSettings, (MobileValidationSettings x) => x.CustomerAttributeId, true, activeStoreScopeConfiguration, false).Wait();

                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.MobileValidationActiveCode, model.MobileValidationActiveCode_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.MobileValidationNewPass, model.MobileValidationNewPass_OverrideForStore, activeStoreScopeConfiguration, false).Wait();
                this._settingService.SaveSettingOverridablePerStoreAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsIsUseCustomLogin, false, activeStoreScopeConfiguration, false).Wait();

                this._settingService.SaveSettingAsync<MobileValidationSettings, string>(MobileValidationSettings, (MobileValidationSettings x) => x.WebServiceSupport).Wait();
                this._settingService.SaveSettingAsync<MobileValidationSettings, bool>(MobileValidationSettings, (MobileValidationSettings x) => x.SmsIsUseCustomLogin).Wait();
            }
            else
            {
                this._settingService.SaveSettingAsync(MobileValidationSettings).Wait();
            }
            this._settingService.ClearCacheAsync().Wait();
            _notificationService.SuccessNotification(this._localizationService.GetResourceAsync("Admin.Plugins.Saved").Result, true);
            return this.Configure();
        }

        [Area("Admin")]
        [HttpGet, AuthorizeAdmin]
        [ActionName("Configure"), FormValueRequired(new string[]
        {
            "SmsRemaining"
        }), HttpPost]

        public ActionResult UpdateRemainingSms(ConfigurationModel model)
        {
            MobileValidationSettings MobileValidationSettings = this._settingService.LoadSettingAsync<MobileValidationSettings>(0).Result;
            int num = CurrentSmsService.SmsService.RemainingSms(MobileValidationSettings);
            if (num == -1)
            {
                _notificationService.ErrorNotification("خطا در بروزرسانی به وجود آمد.", true);
                EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Error, "get Remaining Sms InjaKala == -1", null, null).Wait();
            }
            MobileValidationSettings.SmsRemaining = num;
            this._settingService.SaveSettingAsync<MobileValidationSettings>(MobileValidationSettings, 0).Wait();

            return this.Configure();
        }
    }
}