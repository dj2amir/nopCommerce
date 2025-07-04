using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InjaKalaIR.Plugin.MobileValidation.Models
{
    public class OtpModel
    {
        public UserRegistrationType RegistrationType { get; set; }
        public bool DisplayCaptcha { get; set; }

        [NopResourceDisplayName("Account.Fields.Phone")]
        public string Mobile { get; set; }

        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.Fields.Password")]
        public string Password { get; set; }

        [DataType(DataType.Text)]
        [NopResourceDisplayName("Account.Fields.OTP")]
        public string Otp { get; set; }

        public int levelNo { get; set; }
        public int? StepNext { get; set; }

        public string ReturnUrl { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public bool NeedPassword { get; set; } = false;

        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public bool NeedEmailConfirmation { get; set; } = false;

        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public bool NeedOtp { get; set; } = true;
    }
}