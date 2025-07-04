using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InjaKalaIR.Plugin.MobileValidation.Models
{
    public record LoginLightBoxModel : BaseNopModel
    {
        public string StoreName { get; set; }

        public int PhoneNumberActivationCodeLength { get; set; }

        public string ReturnUrl { get; set; }
    }
}