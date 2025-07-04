using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using InjaKalaIR.Plugin.MobileValidation.Models;
using InjaKalaIR.Plugin.MobileValidation.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Controllers
{
    public class OtpController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly CustomerSettings _customerSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly ISettingService _settingService;

        private readonly LocalizationSettings _localizationSettings;

        public OtpController(
            IWorkContext workContext,
            ICustomerService customerService,
            IStoreContext storeContext,
            CustomerSettings customerSettings,
            IShoppingCartService shoppingCartService,
            IWorkflowMessageService workflowMessageService,
            ICustomerRegistrationService customerRegistrationService,
            IGenericAttributeService genericAttributeService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IAuthenticationService authenticationService,
            IRepository<GenericAttribute> genericAttributeRepository,
            ISettingService settingService,
            LocalizationSettings localizationSettings)
        {
            this._workContext = workContext;
            this._customerService = customerService;
            this._storeContext = storeContext;
            this._customerSettings = customerSettings;
            this._shoppingCartService = shoppingCartService;
            this._workflowMessageService = workflowMessageService;
            this._customerRegistrationService = customerRegistrationService;
            this._genericAttributeService = genericAttributeService;
            this._eventPublisher = eventPublisher;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._authenticationService = authenticationService;
            this._genericAttributeRepository = genericAttributeRepository;
            this._settingService = settingService;
            this._localizationSettings = localizationSettings;
        }

        public IActionResult Index()
        {
            return View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaOtp/Index.cshtml", new LoginLightBoxModel() { PhoneNumberActivationCodeLength = 4 });
        }

        [HttpPost]
        public async Task<IActionResult> OtpGetStatus(string Mobile)
        {
            var customer = await this._customerService.GetCustomerByUsernameAsync(Mobile);
            if (customer == null)
                return Json(new { step = "1" });
            else
                return Json(new { step = "2" });
        }

        [HttpPost]
        public async Task<IActionResult> OtpSendCode(OtpModel model, string Mobile, string Email)
        {
            var customer = await this._customerService.GetCustomerByUsernameAsync(Mobile);
            if (customer == null)
                customer = await this._customerService.GetCustomerByEmailAsync(Email);
            //await EngineContext.Current.Resolve<ILogger>().InformationAsync("Y1"+ Mobile+" "+ model.Mobile);
            var otpRequestedTimestampStr = _genericAttributeRepository.Table.FirstOrDefault(
                                            a => a.KeyGroup == nameof(Customer)
                                            && a.Key == "PhoneNumberActivionTimeStampRequested" &&
                                            a.EntityId == customer.Id);

            var otpRequestedTimestamp = int.Parse(otpRequestedTimestampStr?.Value ?? "0");
            var nowTimestamp = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds));
            var timeBetween = nowTimestamp - otpRequestedTimestamp;

            //SecondValidCodeDuration
            if (timeBetween <= 60)
            {
                return Json(new { ErrorList = "امکان ارسال کد نیست." });
            }
            MobileValidationSettings MobileValidationSettings = await
                this._settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id);

            var token = CommonHelper.GenerateRandomNumber(4).ToString();
            var Message = string.Format(MobileValidationSettings.MobileValidationActiveCode, token.ToPersianNumbers());
            var msg = new Envelope()
            {
                Message = Message,
                To = customer.Username
            };

            if (CurrentSmsService.SmsService.GetType().IsAssignableFrom(typeof(KavenegarTokenSmsService)))
            {
                msg.Message = string.Format(msg.Message, token.ToPersianNumbers());
            }

            await EngineContext.Current.Resolve<ILogger>().InformationAsync("msg:" + msg.Message);
            CurrentSmsService.SmsService.SendSms(MobileValidationSettings, new System.Collections.Generic.List<Envelope>() { msg });

            await _genericAttributeService.SaveAttributeAsync(customer, "LatestPhoneNumberActivationCode", token);
            await _genericAttributeService.SaveAttributeAsync(customer, "PhoneNumberActivionTimeStampRequested", nowTimestamp.ToString());

            return Json(new { Success = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> OtpLoginFull(OtpModel model)
        {
            bool success = false;
            var resultModel = new GeneralResponseModel<OtpLoginFullResultModel>();

            var oldCustomerCart = this._shoppingCartService.GetShoppingCartAsync(_workContext.GetCurrentCustomerAsync().Result).Result.Sum(a => a.Quantity);
            model.Mobile = model.Mobile.ToEnglishNumbers();

           
            /**
             * check user is Logined or Not Logined
             */
            if (await this._customerService.IsRegisteredAsync(_workContext.GetCurrentCustomerAsync().Result))
            {
                success = true;
                resultModel.ErrorList.Add("شما قبلا وارد  شده اید.");
                resultModel.Data.Result.TryAdd("redirect", model.ReturnUrl);
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    resultModel.ErrorList.Add("اطلاعات ارسالی صحیح نیست لطفا خطاهای موجود را برطرف نمایید.");
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (ModelError modelError in allErrors)
                    {
                        resultModel.ErrorList.Add("خطا: " + modelError.ErrorMessage);
                    }
                    return Json(resultModel);
                }
                else
                {
                    int loginType = 1;
                    Customer customer = null;

                    if (model.Mobile.IsValidMobileNumber())
                    {

                        customer = await _customerService.GetCustomerByUsernameAsync(model.Mobile);

                        if (customer == null)
                        {
                            var currentCustomer = await this._workContext.GetCurrentCustomerAsync();
                            //currentCustomer.Username = model.Mobile;
                            //await this._customerService.UpdateCustomerAsync(currentCustomer);

                            model.NeedPassword = false;
                            model.NeedOtp = true;
                            //resultModel.Data.Result.Add("redirect", "/register");
                            //resultModel.Success = true;
                            //return Json(resultModel);
                        }
                        else
                        {
                            model.NeedPassword = true;
                            model.NeedOtp = false;
                            loginType = 2;
                        }
                        //model.NeedPassword = false;

                    }
                    else if (model.Email.IsValidEmail())
                    {

                        customer = await _customerService.GetCustomerByEmailAsync(model.Email);

                        loginType = 2;
                        model.NeedPassword = true;
                        model.NeedOtp = false;
                    }

                    if (customer != null && customer.Deleted)
                    {
                        resultModel.ErrorList.Add("کاربر شما حذف شده است.");
                    }

                    else
                    {

                        bool registerCheck = true;

                        if (customer == null)
                        {
                            customer = await _workContext.GetCurrentCustomerAsync();
                            if (model.Mobile.IsValidMobileNumber())
                            {
                                customer.Username = model.Mobile;
                            }

                            if (model.Email.IsValidEmail())
                            {
                                customer.Email = model.Email;
                                customer.Username = model.Email;
                            }
                            customer.Active = true;
                            customer.RegisteredInStoreId = _storeContext.GetCurrentStore().Id;

                            {
                                model.Password = CommonHelper.GenerateRandomDigitCode(8);
                            }

                            var isApproved = true;
                            if (string.IsNullOrWhiteSpace(model.Email))
                            {
                                var storeUrl = (this._storeContext.GetCurrentStore().Url
                                    .Replace("https:", "").Replace("http:", "").Replace("/", "").Replace("www.", ""));
                                if (storeUrl.Contains("localhost"))
                                    storeUrl = "store.com";
                                model.Email = model.Mobile + "@" + storeUrl;
                            }

                            var registrationRequest = new CustomerRegistrationRequest(customer,
                                model.Email,
                                 model.Mobile ?? model.Email,
                                model.Password,
                                _customerSettings.DefaultPasswordFormat,
                                _storeContext.GetCurrentStore().Id,
                                isApproved);

                            //var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                            await this._customerService.UpdateCustomerAsync(customer);
                           
                        }

                        if (registerCheck)
                        {

                            var validLevels = new List<int> { 1, 2, 3 };
                            if (!validLevels.Contains(model.levelNo))
                            {
                                resultModel.ErrorList.Add("مرحله وارد شده صحیح نيست.");
                            }
                            else
                            {
                                ///5: check levels which

                                var stepCheck = false;

                                if (model.levelNo == 1)
                                {
                                    if (loginType == 1)
                                    {
                                        var otpRequestedTimestampStr = _genericAttributeRepository.Table.FirstOrDefault(
                                            a => a.KeyGroup == nameof(Customer)
                                            && a.Key == "PhoneNumberActivionTimeStampRequested" &&
                                            a.EntityId == customer.Id);
                                        var otpRequestedTimestamp = int.Parse(otpRequestedTimestampStr?.Value ?? "0");
                                        var nowTimestamp = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds));
                                        var timeBetween = nowTimestamp - otpRequestedTimestamp;

                                        MobileValidationSettings MobileValidationSettings = await
                                            this._settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id);


                                        var token = CommonHelper.GenerateRandomNumber(4).ToString();
                                        var Message = string.Format(MobileValidationSettings.MobileValidationActiveCode, token.ToPersianNumbers());
                                        var msg = new Envelope()
                                        {
                                            Message = Message,
                                            To = model.Mobile
                                        };
                                        if (CurrentSmsService.SmsService.GetType().IsAssignableFrom(typeof(KavenegarTokenSmsService)))
                                        {
                                            //msg.Message = token.ToPersianNumbers();
                                            msg.Message = string.Format(msg.Message, token.ToPersianNumbers());
                                        }

                                        await EngineContext.Current.Resolve<ILogger>().InformationAsync("msg:" + msg.Message);
                                        CurrentSmsService.SmsService.SendSms(MobileValidationSettings, new System.Collections.Generic.List<Envelope>() { msg });

                                        await _genericAttributeService.SaveAttributeAsync(customer, "LatestPhoneNumberActivationCode", token);
                                        await _genericAttributeService.SaveAttributeAsync(customer, "PhoneNumberActivionTimeStampRequested", nowTimestamp.ToString());

                                        stepCheck = true;
                                        model.StepNext = 3;
                                    }
                                    else if (loginType == 2)
                                    {
                                        stepCheck = true;
                                        model.StepNext = 2;
                                    }
                                }
                                else if (model.levelNo == 2)
                                {
                                    if (loginType == 2)
                                    {
                                        var loginResult = await _customerRegistrationService.ValidateCustomerAsync(model.Email?? model.Mobile, model.Password);
                                        //await EngineContext.Current.Resolve<ILogger>().InformationAsync("AA:" + model.Email+" "+ model.Password);
                                        switch (loginResult)
                                        {
                                            case CustomerLoginResults.Successful:
                                                //migrate shopping cart
                                                await _shoppingCartService.MigrateShoppingCartAsync(_workContext.GetCurrentCustomerAsync().Result, customer, true);

                                                customer.Active = true;
                                                await this._customerService.UpdateCustomerAsync(customer);
                                                //sign in new customer
                                                await _authenticationService.SignInAsync(customer, true);

                                                //raise event
                                                await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                                                //activity log
                                                await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                                                    await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

                                                stepCheck = true;

                                                break;

                                            case CustomerLoginResults.Deleted:
                                            case CustomerLoginResults.NotActive:
                                            case CustomerLoginResults.LockedOut:
                                                resultModel.ErrorList.Add("حساب کاربری شما در دسترس نمی باشد.");
                                                break;

                                            case CustomerLoginResults.WrongPassword:
                                            default:
                                                resultModel.ErrorList.Add("رمز عبور وارد شده صحیح نمی باشد.");
                                                break;
                                        }
                                    }
                                }
                                else if (model.levelNo == 3)
                                {
                                    var otpRequestedTimestampStr = _genericAttributeRepository.Table.FirstOrDefault(
                                            a => a.KeyGroup == nameof(Customer)
                                            && a.Key == "PhoneNumberActivionTimeStampRequested" &&
                                            a.EntityId == customer.Id);
                                    var otpRequestedTimestamp = int.Parse(otpRequestedTimestampStr.Value);
                                    var nowTimestamp = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds));
                                    var timeBetween = nowTimestamp - otpRequestedTimestamp;
                                    if (timeBetween <= 60)
                                    {
                                        var activationCodeAttribute = _genericAttributeRepository.Table.FirstOrDefault(
                                            a => a.KeyGroup == nameof(Customer)
                                            && a.Key == "LatestPhoneNumberActivationCode" &&
                                            a.EntityId == customer.Id);
                                        if (activationCodeAttribute.Value != model.Otp.ToEnglishNumbers())
                                        {
                                            resultModel.ErrorList.Add("کد ورود صحیح نمی باشد.");
                                        }
                                        else
                                        {
                                            customer.Active = true;
                                            await this._customerService.UpdateCustomerAsync(customer);

                                            await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);
                                            await _authenticationService.SignInAsync(customer, true);
                                            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

                                            //activity log
                                            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                                                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);
                                            stepCheck = true;
                                        }
                                    }
                                    else
                                    {
                                        resultModel.ErrorList.Add("کد وارد شده منقضی شده است. لطفا مجددا مراحل را طی نمایید..");
                                    }
                                }
                                if (stepCheck)
                                {
                                    success = true;
                                    if (model.levelNo == 2 || model.levelNo == 3)
                                    {
                                        model.NeedOtp = false;
                                        model.NeedPassword = false;
                                        model.NeedEmailConfirmation = false;

                                        if (string.IsNullOrEmpty(model.ReturnUrl))
                                        {
                                            model.ReturnUrl = "/";
                                        }
                                        //if (string.IsNullOrEmpty(await this._customerService.GetCustomerFullNameAsync(customer)))
                                        //{
                                        //    model.ReturnUrl = "/register";
                                        //}

                                        if (model.ReturnUrl.Equals("loginregister"))
                                        {
                                            if (oldCustomerCart > 0 && oldCustomerCart != this._shoppingCartService.GetShoppingCartAsync(customer).Result.Sum(a => a.Quantity))
                                            {
                                                model.ReturnUrl = "/cart";
                                            }
                                            else
                                            {
                                                model.ReturnUrl = "/checkout/billingaddress";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        model.ReturnUrl = null;
                                    }

                                    if (model.StepNext.HasValue)
                                    {
                                        resultModel.Data.Result.Add("number_levelNo", model.StepNext.ToString());
                                    }
                                    if (model.NeedEmailConfirmation)
                                    {
                                        resultModel.Data.Result.Add("need_email_confirmation", "true");
                                    }
                                    if (model.NeedOtp)
                                    {
                                        resultModel.Data.Result.Add("need_otp", "true");
                                    }
                                    if (model.NeedPassword)
                                    {
                                        resultModel.Data.Result.Add("need_password", "true");
                                    }

                                    if (model.ReturnUrl?.Length > 0)
                                    {
                                        resultModel.Data.Result.Add("redirect", model.ReturnUrl);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            resultModel.Success = success;

            return Json(resultModel);
        }
    }
}