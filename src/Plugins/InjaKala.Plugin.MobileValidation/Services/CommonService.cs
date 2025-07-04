using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Orders;
using Nop.Services.Events;
using Nop.Services.Configuration;
using InjaKalaIR.Plugin.MobileValidation.Service;
using InjaKalaIR.Plugin.MobileValidation.Models;
using Nop.Web.Factories;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Core.Events;

namespace InjaKalaIR.Plugin.MobileValidation.Services
{
    public class CommonService
    {
        public CommonService()
        {
            this._settingService = EngineContext.Current.Resolve<ISettingService>();
            this._customerService = EngineContext.Current.Resolve<ICustomerService>();
            this._customerModelFactory = EngineContext.Current.Resolve<ICustomerModelFactory>();
            this._genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            this._storeContext = EngineContext.Current.Resolve<IStoreContext>();
            this._eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
            this._encryptionService = EngineContext.Current.Resolve<IEncryptionService>();
            this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            this._customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
        }

        //public CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        //{
        //    ILocalizationService _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        //    CustomerSettings _customerSettings = EngineContext.Current.Resolve<CustomerSettings>();

        //    var _encryptionService = EngineContext.Current.Resolve<IEncryptionService>();
        //    var _rewardPointsSettings = EngineContext.Current.Resolve<RewardPointsSettings>();
        //    var _rewardPointService = EngineContext.Current.Resolve<IRewardPointService>();
        //    var _eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();

        //    if (request == null)
        //        throw new ArgumentNullException("CustomerRegistrationRequest");

        //    if (request.Customer == null)
        //        throw new ArgumentException("Can't load current customer");

        //    var result = new CustomerRegistrationResult();
        //    if (request.Customer.IsSearchEngineAccount())
        //    {
        //        result.AddError("Search engine can't be registered");
        //        return result;
        //    }
        //    if (request.Customer.IsBackgroundTaskAccount())
        //    {
        //        result.AddError("Background task account can't be registered");
        //        return result;
        //    }

        //    if (this._customerService.IsRegistered(request.Customer))
        //    {
        //        result.AddError("Current customer is already registered");
        //        return result;
        //    }
        //    //default password MJVAKILI
        //    if (string.IsNullOrWhiteSpace(request.Email))
        //    {
        //        request.Email = request.Username + "@" + (this._storeContext.CurrentStore.Url.Replace("https:", "").Replace("http:", "").Replace("/", "").Replace("www.", ""));

        //    }
        //    //if (string.IsNullOrEmpty(request.Email))
        //    //{
        //    //    result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided"));
        //    //    return result;
        //    //}
        //    if (!string.IsNullOrEmpty(request.Email) && !CommonHelper.IsValidEmail(request.Email))
        //    {
        //        result.AddError(_localizationService.GetResource("Common.WrongEmail"));
        //        return result;
        //    }
        //    if (string.IsNullOrWhiteSpace(request.Password))
        //    {
        //        result.AddError(_localizationService.GetResource("Account.Register.Errors.PasswordIsNotProvided"));
        //        return result;
        //    }
        //    if (_customerSettings.UsernamesEnabled)
        //    {
        //        if (string.IsNullOrEmpty(request.Username))
        //        {
        //            result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
        //            return result;
        //        }
        //    }

        //    //validate unique user
        //    //if (_customerService.GetCustomerByEmail(request.Email) != null)

        //    if (_customerSettings.UsernamesEnabled)
        //    {
        //        if (_customerService.GetCustomerByUsername(request.Username) != null)
        //        {
        //            result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists"));
        //            return result;
        //        }
        //    }
        //    else if (_customerService.GetCustomerByUsername(request.Username) != null)
        //    {
        //        result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists"));
        //        return result;
        //    }

        //    //at this point request is valid
        //    request.Customer.Username = request.Username;
        //    request.Customer.Email = request.Email;

        //    var customerPassword = new CustomerPassword
        //    {
        //        CustomerId = request.Customer.Id,
        //        PasswordFormat = request.PasswordFormat,
        //        CreatedOnUtc = DateTime.UtcNow
        //    };
        //    switch (request.PasswordFormat)
        //    {
        //        case PasswordFormat.Clear:
        //            customerPassword.Password = request.Password;
        //            break;
        //        case PasswordFormat.Encrypted:
        //            customerPassword.Password = _encryptionService.EncryptText(request.Password);
        //            break;
        //        case PasswordFormat.Hashed:
        //            {
        //                var saltKey = _encryptionService.CreateSaltKey(5);
        //                customerPassword.PasswordSalt = saltKey;
        //                customerPassword.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _customerSettings.HashedPasswordFormat);
        //            }
        //            break;
        //    }
        //    _customerService.InsertCustomerPassword(customerPassword);

        //    request.Customer.Active = request.IsApproved;

        //    //add to 'Registered' role
        //    var registeredRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
        //    if (registeredRole == null)
        //        throw new NopException("'Registered' role could not be loaded");
        //    request.Customer.CustomerRoles.Add(registeredRole);
        //    //remove from 'Guests' role
        //    var guestRole = request.Customer.CustomerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.GuestsRoleName);
        //    if (guestRole != null)
        //        request.Customer.CustomerRoles.Remove(guestRole);

        //    //Add reward points for customer registration (if enabled)
        //    if (_rewardPointsSettings.Enabled &&
        //        _rewardPointsSettings.PointsForRegistration > 0)
        //    {
        //        _rewardPointService.AddRewardPointsHistoryEntry(request.Customer,
        //            _rewardPointsSettings.PointsForRegistration,
        //            request.StoreId,
        //            _localizationService.GetResource("RewardPoints.Message.EarnedForRegistration"));
        //    }

        //    //EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Information, "MJV 881 " + request.IsApproved);
        //    _customerService.UpdateCustomer(request.Customer);

        //    //publish event
        //    _eventPublisher.Publish(new CustomerPasswordChangedEvent(customerPassword));

        //    return result;
        //}

        public void SendSms(int CustomerId)
        {
            MobileValidationSettings MobileValidationSettings = this._settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id).Result;
            if (MobileValidationSettings.SmsIsSendValidation)
            {
                var customer = _customerService.GetCustomerByIdAsync(CustomerId).Result;
                //customer.Active = false;
                //_customerService.UpdateCustomer(customer);

                //EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Information, "MJV Test2 ", resultId + "-" + customer.Id + "-" + customer.Username + "-" + MobileValidationSettings.SmsIsActive + " " + MobileValidationSettings.SmsUsername, null);
                Random rnd = new Random();
                string randomCode = rnd.Next(952).ToString();
                //base.HttpContext.Session.SetString("codeSms", randomCode);

                _genericAttributeService.SaveAttributeAsync(customer, "MV_Code",
                         randomCode).Wait();
                //DateTime? generatedDateTime = DateTime.UtcNow;
                //_genericAttributeService.SaveAttribute(customer,
                //    SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

                //EngineContext.Current.Resolve<ILogger>().Warning("SSS2");
                var msg = new Envelope()
                {
                    Message = "کد فعالسازی:" +
                    randomCode,
                    To = customer.Username
                };
                if (CurrentSmsService.SmsService.GetType().IsAssignableFrom(typeof(KavenegarTokenSmsService)))
                {
                    msg.Message = string.Format(msg.Message, randomCode);
                }
                CurrentSmsService.SmsService.SendSms(MobileValidationSettings, new System.Collections.Generic.List<Envelope>() { msg });
                //return RedirectToAction("RegistrationVerification", new { username = customer.Username });
            }
        }

        public bool IsValidCode(Customer customer, string Code)
        {
            //var customer = this._customerService.GetCustomerByUsername(UserName);
            //EngineContext.Current.Resolve<ILogger>().Warning("E1:" + UserName + " " + Code + " " + customer.Id);
            return Code == this._genericAttributeService.GetAttributeAsync<string>(customer, "MV_Code").Result;
        }

        public bool IsValidationActive()
        {
            return this._settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id).Result.SmsIsSendValidation;
        }

        public bool PasswordRecovery(string Mobile, out string result)
        {
            var customer = _customerService.GetCustomerByUsernameAsync(Mobile).Result;
            result = "ورود اطلاعات معتبر نمی باشد.";

            if (customer != null //&& customer.Active
                && !customer.Deleted)
            {
                Random rnd = new Random();
                string randomCode = rnd.Next(99522).ToString();

                MobileValidationSettings MobileValidationSettings = this._settingService.LoadSettingAsync<MobileValidationSettings>(_storeContext.GetCurrentStore().Id).Result;
                var msg = new Envelope()
                {
                    Message = string.Format(_settingService.GetSettingByKeyAsync<string>("MobileValidationNewPass").Result, randomCode),
                    To = Mobile
                };
                if (CurrentSmsService.SmsService.GetType().IsAssignableFrom(typeof(KavenegarTokenSmsService)))
                {
                    msg.Message = string.Format(msg.Message, randomCode);
                    //msg.Message = randomCode;
                    //MobileValidationSettings.SmsLineNumber = "forget";
                }

                CurrentSmsService.SmsService.SendSms(MobileValidationSettings, new System.Collections.Generic.List<Envelope>() { msg });
                //EngineContext.Current.Resolve<ILogger>().Warning("SSS4");

                ChangePasswordPrivate(new ChangePasswordRequestPrivate(customer.Username,
                    false, _customerSettings.DefaultPasswordFormat, randomCode));
                if (!customer.Active)
                    this._customerService.UpdateCustomerAsync(customer);
                //save token and current date
                // var passwordRecoveryToken = Guid.NewGuid();
                // _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken,
                //     passwordRecoveryToken.ToString());
                // DateTime? generatedDateTime = DateTime.UtcNow;
                // _genericAttributeService.SaveAttribute(customer,
                //     SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

                // //send email
                // _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer,
                //     _workContext.WorkingLanguage.Id);

                result = "رمز عبور جدید ارسال شد.";// _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                return true;
            }
            else
            {
                result = "کاربر یافت نشد.";// _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                return false;
            }
        }

        public class ChangePasswordRequestPrivate
        {
            public ChangePasswordRequestPrivate(string userName, bool validateRequest, PasswordFormat newPasswordFormat, string newPassword, string oldPassword = "")
            {
                this.UserName = userName;
                this.ValidateRequest = validateRequest;
                this.NewPasswordFormat = newPasswordFormat;
                this.NewPassword = newPassword;
                this.OldPassword = oldPassword;
            }

            public string UserName { get; set; }
            public string NewPassword { get; set; }
            public PasswordFormat NewPasswordFormat { get; set; }
            public string OldPassword { get; set; }
            public bool ValidateRequest { get; set; }
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

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public ChangePasswordResult ChangePasswordPrivate(ChangePasswordRequestPrivate request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var result = new ChangePasswordResult();
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                result.AddError(_localizationService.GetResourceAsync("Account.ChangePassword.Errors.EmailIsNotProvided").Result);
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError(_localizationService.GetResourceAsync("Account.ChangePassword.Errors.PasswordIsNotProvided").Result);
                return result;
            }

            var customer = _customerService.GetCustomerByUsernameAsync(request.UserName).Result;
            if (customer == null)
            {
                result.AddError(_localizationService.GetResourceAsync("Account.ChangePassword.Errors.EmailNotFound").Result);
                return result;
            }

            //request isn't valid
            if (request.ValidateRequest && !PasswordsMatch(
                _customerService.GetCurrentPasswordAsync(customer.Id).Result, request.OldPassword))
            {
                result.AddError(_localizationService.GetResourceAsync("Account.ChangePassword.Errors.OldPasswordDoesntMatch").Result);
                return result;
            }

            //check for duplicates
            if (_customerSettings.UnduplicatedPasswordsNumber > 0)
            {
                //get some of previous passwords
                var previousPasswords = _customerService.GetCustomerPasswordsAsync(customer.Id, passwordsToReturn: _customerSettings.UnduplicatedPasswordsNumber).Result;

                var newPasswordMatchesWithPrevious = previousPasswords.Any(password => PasswordsMatch(password, request.NewPassword));
                if (newPasswordMatchesWithPrevious)
                {
                    result.AddError(_localizationService.GetResourceAsync("Account.ChangePassword.Errors.PasswordMatchesWithPrevious").Result);
                    return result;
                }
            }

            //at this point request is valid
            var customerPassword = new CustomerPassword
            {
                CustomerId = customer.Id,
                PasswordFormat = request.NewPasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };

            switch (request.NewPasswordFormat)
            {
                case PasswordFormat.Clear:
                    customerPassword.Password = request.NewPassword;
                    break;

                case PasswordFormat.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.NewPassword);
                    break;

                case PasswordFormat.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(5);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, _customerSettings.HashedPasswordFormat);
                    break;
            }

            _customerService.InsertCustomerPasswordAsync(customerPassword).Wait();

            //publish event
            _eventPublisher.PublishAsync(new CustomerPasswordChangedEvent(customerPassword)).Wait();

            return result;
        }

        public ISettingService _settingService { get; set; }

        public ICustomerService _customerService { get; set; }

        public ICustomerModelFactory _customerModelFactory { get; set; }

        public IGenericAttributeService _genericAttributeService { get; set; }

        public IStoreContext _storeContext { get; set; }

        public IEventPublisher _eventPublisher { get; set; }

        public IEncryptionService _encryptionService { get; set; }

        public ILocalizationService _localizationService { get; set; }

        public CustomerSettings _customerSettings { get; set; }
    }
}