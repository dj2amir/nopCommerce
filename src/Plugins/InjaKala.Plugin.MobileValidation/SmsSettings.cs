using System;
using Nop.Core.Configuration;

namespace InjaKalaIR.Plugin.MobileValidation
{
    public class MobileValidationSettings : ISettings
    {
        public string WebSiteHashPassword
        {
            get;
            set;
        }

        public string SmsUsername
        {
            get;
            set;
        }

        public string SmsPassword
        {
            get;
            set;
        }

        public string SmsLineNumber
        {
            get;
            set;
        }

        public int? SmsRemaining
        {
            get;
            set;
        }

        public string ManagerPhoneNumber
        {
            get;
            set;
        }

        public bool SmsIsActive
        {
            get;
            set;
        }

        public bool SmsIsUseCustomLogin
        {
            get;
            set;
        }

        public bool EnableOtp
        {
            get;
            set;
        }

        public bool SmsIsSendValidation
        {
            get;
            set;
        }

        public bool SmsEmailOptional
        {
            get;
            set;
        }

        public bool SmsFirstNameOptional
        {
            get;
            set;
        }

        public int TaskId
        {
            get;
            set;
        }

        public int TaskDailyId
        {
            get;
            set;
        }

        public int CustomerAttributeId
        {
            get;
            set;
        }

        public string SmsSubject
        {
            get;
            set;
        }

        public bool IsCustomerSms
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.CustomerContent);
            }
        }

        public string CustomerContent
        {
            get;
            set;
        }

        public bool IsBirthdaySms
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.BirthdayContent);
            }
        }

        public string BirthdayContent
        {
            get;
            set;
        }

        public bool IsAbandonedSms
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.AbandonedContent);
            }
        }

        public string AbandonedContent
        {
            get;
            set;
        }

        public int AbandonedDay { get; set; }

        public bool IsOrderSms
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.OrderContent);
            }
        }

        public string OrderContent
        {
            get;
            set;
        }

        public bool IsShippingSms
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.ShippingContent);
            }
        }

        public string ShippingContent
        {
            get;
            set;
        }

        public string MobileValidationActiveCode { get; set; }
        public string MobileValidationNewPass { get; set; }
        public string WebServiceSupport { get; set; }
    }
}