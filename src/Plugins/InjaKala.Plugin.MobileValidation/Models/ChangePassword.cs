using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InjaKalaIR.Plugin.MobileValidation.Models
{
    public class ChangePassword
    {
        [NopResourceDisplayName("کد اعتبارسنجی")]
        public string CodeSend { get; set; }

        [NopResourceDisplayName("تلفن فعلی شما")]
        public string LastUserName { get; set; }

        public string Result { get; set; }

        [NopResourceDisplayName("تلفن جدید شما")]
        public string UserName { get; set; }
    }
}