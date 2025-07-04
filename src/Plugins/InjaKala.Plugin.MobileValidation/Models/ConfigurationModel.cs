using System;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InjaKalaIR.Plugin.MobileValidation.Models
{
    public class ConfigurationModel //: BaseNopModel
    {
        public int ActiveStoreScopeConfiguration
        {
            get;
            set;
        }

        public string Error
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.WebSiteHashCode")]
        public string WebSiteHashCode
        {
            get;
            set;
        }

        public bool WebSiteHashCode_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsUsername")]
        public string SmsUsername
        {
            get;
            set;
        }

        public bool SmsUsername_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsPassword")]
        [DataType(DataType.Text)]
        public string SmsPassword
        {
            get;
            set;
        }

        public bool SmsPassword_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.LineNumber")]
        public string LineNumber
        {
            get;
            set;
        }

        public bool LineNumber_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsRemaining")]
        public int? SmsRemaining
        {
            get;
            set;
        }

        public bool SmsRemaining_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsSubject")]
        public string SmsSubject
        {
            get;
            set;
        }

        public bool SmsSubject_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsIsActive")]
        public bool SmsIsActive
        {
            get;
            set;
        }

        public bool SmsIsActive_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.IsValidation")]
        public bool SmsIsSendValidation
        {
            get;
            set;
        }

        public bool SmsIsSendValidation_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsTestText")]
        public string SmsTestText
        {
            get;
            set;
        }

        public string InjaKala
        {
            get;
            set;
        }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsFirstNameOptional")]
        public bool SmsFirstNameOptional { get; set; }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsEmailOptional")]
        public bool SmsEmailOptional { get; set; }

        public bool SmsFirstNameOptional_OverrideForStore { get; set; }

        public bool SmsEmailOptional_OverrideForStore { get; set; }

        [NopResourceDisplayName("InjaKala.MobileValidation.MobileValidationActiveCode")]
        public string MobileValidationActiveCode { get; set; }

        public bool MobileValidationActiveCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("InjaKala.MobileValidation.MobileValidationNewPass")]
        public string MobileValidationNewPass { get; set; }

        public bool MobileValidationNewPass_OverrideForStore { get; set; }

        [NopResourceDisplayName("InjaKala.MobileValidation.WebServiceSupport")]
        public string WebServiceSupport { get; set; }

        [NopResourceDisplayName("InjaKala.MobileValidation.SmsIsUseCustomLogin")]
        public bool SmsIsUseCustomLogin { get; set; }

        [NopResourceDisplayName("InjaKala.MobileValidation.EnableOtp")]
        public bool EnableOtp { get; set; }

        public bool EnableOtp_OverrideForStore
        {
            get;
            set;
        }
    }
}