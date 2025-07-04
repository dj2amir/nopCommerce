using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;
using Nop.Services.Stores;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Authentication;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Tax;
using Nop.Web.Factories;
using InjaKalaIR.Plugin.MobileValidation.Models;
using InjaKalaIR.Plugin.MobileValidation.Service;
using InjaKalaIR.Plugin.MobileValidation.Services;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Framework.Mvc.Filters; // For HttpsRequirement, CheckAccessClosedStore, CheckAccessPublicStore, ValidateCaptcha
using Nop.Web.Models.Customer; // For LoginModel
using Nop.Core.Domain.Security; // For CaptchaSettings
using Nop.Core.Domain.Tax; // For TaxSettings
using Nop.Services.Events; // For IEventPublisher
using Nop.Web.Controllers; // For BasePublicController
using Nop.Core.Events; // For IEventPublisher
using Nop.Core.Infrastructure; // For EngineContext
using Nop.Data; // For IRepository

namespace InjaKalaIR.Plugin.MobileValidation.Controllers
{
    public partial class InjaKalaCustomerValidationController : BasePublicController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly TaxSettings _taxSettings;
        private readonly IEncryptionService _encryptionService;

        #endregion Fields

        #region Ctor

        public InjaKalaCustomerValidationController(
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            IAddressService addressService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ISettingService settingService,
            IStoreContext storeContext,
            ICustomerModelFactory customerModelFactory,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            IShoppingCartService shoppingCartService,
            IAuthenticationService authenticationService,
            IEventPublisher eventPublisher,
            ICustomerActivityService customerActivityService,
            IWorkflowMessageService workflowMessageService,
            DateTimeSettings dateTimeSettings,
            IDateTimeHelper dateTimeHelper,
            TaxSettings taxSettings,
            IEncryptionService encryptionService)
        {
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _addressService = addressService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _settingService = settingService;
            _storeContext = storeContext;
            _customerModelFactory = customerModelFactory;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _shoppingCartService = shoppingCartService;
            _authenticationService = authenticationService;
            _eventPublisher = eventPublisher;
            _customerActivityService = customerActivityService;
            _workflowMessageService = workflowMessageService;
            _dateTimeSettings = dateTimeSettings;
            _dateTimeHelper = dateTimeHelper;
            _taxSettings = taxSettings;
            _encryptionService = encryptionService;
        }

        #endregion Ctor

        #region Utilities

        private string toPersianNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            string[] persian = new string[10] { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" };
            for (int j = 0; j < persian.Length; j++)
                input = input.Replace(persian[j], j.ToString());
            return input;
        }

        #endregion Utilities

        #region Methods

        #region Login / logout

        public IActionResult NFChangePhone()
        {
            var customer = _workContext.GetCurrentCustomerAsync().Result;
            ChangePassword result = new ChangePassword() { LastUserName = customer.Username };
            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/NFChangePhone.cshtml", result);
        }

        [HttpPost]
        public IActionResult NFChangePhone(ChangePassword model)
        {
            var customer = _workContext.GetCurrentCustomerAsync().Result;
            model.LastUserName = customer.Username;
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("codeValidation")))
            {
                if (HttpContext.Session.GetString("codeValidation") != model.CodeSend)
                {
                    model.Result = "کد ارسالی معتبر نیست.";
                }
                else
                {
                    var rcc = EngineContext.Current.Resolve<IRepository<Customer>>();
                    var cc = rcc.Table.FirstOrDefault(x => x.Id == customer.Id);
                    cc.Username = HttpContext.Session.GetString("PhoneNumberMV");

                    rcc.UpdateAsync(cc).Wait();

                    model.Result = "با موفقیت بروز رسانی شد.";
                    HttpContext.Session.SetString("codeValidation", "");
                }
            }
            else
            {
                Random rnd = new Random();
                string randomCode = rnd.Next(99589).ToString();

                MobileValidationSettings MobileValidationSettings = _settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id).Result;
                var sms = MobileValidationSettings.MobileValidationActiveCode;
                var msg = new Envelope()
                {
                    Message = string.Format(sms, randomCode),
                    To = model.UserName
                };

                var result = CurrentSmsService.SmsService.SendSms(MobileValidationSettings, new System.Collections.Generic.List<Envelope>() { msg });

                HttpContext.Session.SetString("codeValidation", randomCode);
                HttpContext.Session.SetString("PhoneNumberMV", model.UserName);
                model.Result = "پیام ارسال شد.";
            }

            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/NFChangePhone.cshtml", model);
        }

        [HttpsRequirement]
        [CheckAccessClosedStore(true)]
        [CheckAccessPublicStore(true)]
        public virtual IActionResult NFLogin(bool? checkoutAsGuest)
        {
            MobileValidationSettings MobileValidationSettings = _settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id).Result;
            if (MobileValidationSettings.EnableOtp)
                return RedirectToRoute("InjaKalaOtp");

            var model = _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest).Result;
            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/Login.cshtml", model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [CheckAccessClosedStore(true)]
        [CheckAccessPublicStore(true)]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> NFLogin(LoginModel model, string returnUrl, bool captchaValid)
        {
            model.Email = toPersianNumber(model.Email);
            model.Username = toPersianNumber(model.Username);
            model.Password = toPersianNumber(model.Password);

            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult = await ValidateCustomerAsync(string.IsNullOrWhiteSpace(model.Username) ? model.Email : model.Username, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = await _customerService.GetCustomerByUsernameAsync(model.Username);
                            if (customer == null)
                                customer = await _customerService.GetCustomerByEmailAsync(model.Username);

                            await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                            await _authenticationService.SignInAsync(customer, model.RememberMe);

                            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("Homepage");

                            return Redirect(returnUrl);
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;

                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                        break;

                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                        break;

                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                        break;

                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                        break;

                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                        break;
                }
            }

            model = await _customerModelFactory.PrepareLoginModelAsync(model.CheckoutAsGuest);
            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/Login.cshtml", model);
        }

        public virtual async Task<CustomerLoginResults> ValidateCustomerAsync(string usernameOrEmail, string password)
        {
            var customer = await _customerService.GetCustomerByUsernameAsync(usernameOrEmail);
            if (customer == null)
                customer = await _customerService.GetCustomerByEmailAsync(usernameOrEmail);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;
            if (!await _customerService.IsRegisteredAsync(customer))
                return CustomerLoginResults.NotRegistered;
            if (customer.CannotLoginUntilDateUtc.HasValue && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return CustomerLoginResults.LockedOut;

            if (!PasswordsMatch(await _customerService.GetCurrentPasswordAsync(customer.Id), password))
            {
                customer.FailedLoginAttempts++;
                if (_customerSettings.FailedPasswordAllowedAttempts > 0 &&
                    customer.FailedLoginAttempts >= _customerSettings.FailedPasswordAllowedAttempts)
                {
                    customer.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(_customerSettings.FailedPasswordLockoutMinutes);
                    customer.FailedLoginAttempts = 0;
                }

                await _customerService.UpdateCustomerAsync(customer);

                return CustomerLoginResults.WrongPassword;
            }

            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.RequireReLogin = false;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            await _customerService.UpdateCustomerAsync(customer);

            return CustomerLoginResults.Successful;
        }

        protected bool PasswordsMatch(CustomerPassword customerPassword, string enteredPassword)
        {
            if (customerPassword == null || string.IsNullOrEmpty(enteredPassword))
                return false;

            var savedPassword = string.Empty;
            switch (customerPassword.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    savedPassword = enteredPassword;
                    break;
                case PasswordFormat.Encrypted:
                    savedPassword = _encryptionService.EncryptText(enteredPassword);
                    break;
                case PasswordFormat.Hashed:
                    savedPassword = _encryptionService.CreatePasswordHash(enteredPassword, customerPassword.PasswordSalt, _customerSettings.HashedPasswordFormat);
                    break;
            }

            if (customerPassword.Password == null)
                return false;

            return customerPassword.Password.Equals(savedPassword);
        }

        #endregion Login / logout

        #region Password recovery

        [HttpsRequirement]
        [CheckAccessPublicStore(true)]
        public virtual IActionResult NFPasswordRecovery()
        {
            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/PasswordRecovery.cshtml", new InjaKalaPasswordRecoveryModel());
        }

        [HttpPost]
        [CheckAccessPublicStore(true)]
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> NFPasswordRecovery(InjaKalaPasswordRecoveryModel model)
        {
            model.Result = "ورود اطلاعات معتبر نمی باشد.";
            if (ModelState.IsValid)
            {
                string str = HttpContext.Session.GetString("SendPassRecovery");
                DateTime dateTimeSend;
                DateTime.TryParse(str, out dateTimeSend);
                if (!string.IsNullOrEmpty(str) && DateTime.Now.AddMinutes(-1) < dateTimeSend)
                {
                    model.Result = "محدودیت ارسال پیامک مجدد به مدت یک دقیقه";
                    return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/PasswordRecovery.cshtml", model);
                }

                var customer = await _customerService.GetCustomerByUsernameAsync(model.Mobile);

                if (customer != null && !customer.Deleted)
                {
                    ViewBag.ShowSectionActiveCode = true;
                    Random rnd = new Random();
                    string randomCode = rnd.Next(99521).ToString();

                    MobileValidationSettings MobileValidationSettings = await _settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id);
                    var message = MobileValidationSettings.MobileValidationActiveCode;
                    var msg = new Envelope()
                    {
                        Message = string.Format(message, randomCode),
                        To = model.Mobile
                    };

                    CurrentSmsService.SmsService.SendSms(MobileValidationSettings, new System.Collections.Generic.List<Envelope>() { msg });

                    await _genericAttributeService.SaveAttributeAsync(customer, "PasswordRecovery", randomCode);

                    var sendPass = await _settingService.GetSettingByKeyAsync<bool>("MobileValidation.SendPassword");
                    if (sendPass == true)
                    {
                        customer.Active = true;
                        new CommonService().ChangePasswordPrivate(new InjaKalaIR.Plugin.MobileValidation.Services.CommonService.ChangePasswordRequestPrivate(customer.Username,
                            false, _customerSettings.DefaultPasswordFormat, randomCode));

                        var passwordRecoveryToken = Guid.NewGuid();
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                            passwordRecoveryToken.ToString());
                        DateTime? generatedDateTime = DateTime.UtcNow;
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);
                        await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                            (await _workContext.GetWorkingLanguageAsync()).Id);
                    }
                    HttpContext.Session.SetString("SendPassRecovery", DateTime.Now.ToString());
                    model.Result = "رمز فعالسازی ارسال شد.";
                }
                else
                {
                    model.Result = "کاربر یافت نشد.";
                }

                return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/PasswordRecovery.cshtml", model);
            }

            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/PasswordRecovery.cshtml", model);
        }

        [HttpPost, ActionName("NFPasswordRecovery")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("confitm-code")]
        [CheckAccessPublicStore(true)]
        public async Task<IActionResult> NFPasswordRecovery2(InjaKalaPasswordRecoveryModel model, int d)
        {
            ViewBag.ShowSectionActiveCode = true;
            var customer = await _customerService.GetCustomerByUsernameAsync(model.Mobile);

            var token = await _genericAttributeService.GetAttributeAsync<string>(customer, "PasswordRecovery");
            if (customer == null)
            {
                model.Result = "امکان تغییر وجود ندارد.";
            }
            else if (model.NewPassword != model.Password)
            {
                model.Result = "رمز با تایید رمز یکسان نیست.";
            }
            else if (token == model.Token)
            {
                var rr = new CommonService().ChangePasswordPrivate(new InjaKalaIR.Plugin.MobileValidation.Services.CommonService.ChangePasswordRequestPrivate(customer.Username,
                    false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
                customer.Active = true;
                await _customerService.UpdateCustomerAsync(customer);

                if (rr.Success)
                {
                    model.Result += "رمز با موفقیت تغییر کرد. لطفا مجدد وارد شوید.";
                    ViewBag.SuccessChange = true;
                }
                else
                    foreach (var item in rr.Errors)
                    {
                        model.Result += " " + item;
                    }
            }
            else
            {
                model.Result = "کد اشتباه است.";
            }
            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/PasswordRecovery.cshtml", model);
        }

        #endregion Password recovery

        #region Register

        [HttpsRequirement]
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> NFRegister()
        {
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new NFRegisterModel();
            model = await PrepareRegisterModelAsync(model, false, setDefaultValues: true);

            ViewBag.Year = int.Parse(DateTime.Now.ToString("yyyy"));
            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/InjaKalaRegister.cshtml", model);
        }

        public virtual async Task<NFRegisterModel> PrepareRegisterModelAsync(NFRegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == (await _dateTimeHelper.GetCurrentTimeZoneAsync()).Id) });

            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            model.LastNameEnabled = _customerSettings.LastNameEnabled;
            model.FirstNameRequired = _customerSettings.FirstNameRequired;
            model.LastNameRequired = _customerSettings.LastNameRequired;

            return model;
        }

        #endregion Register

        #endregion Methods
    }
}
