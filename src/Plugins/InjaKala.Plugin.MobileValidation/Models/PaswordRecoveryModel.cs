using Nop.Web.Framework;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Validators.Customer;
using System.ComponentModel.DataAnnotations;

namespace InjaKalaIR.Plugin.MobileValidation
{
    public record InjaKalaPasswordRecoveryModel : BaseNopModel
    {
        //[AllowHtml]
        [Required(ErrorMessage = "لطفا شماره موبایل را وارد کنید.")]
        //[DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("InjaKala.Account.PasswordRecovery.Mobile")]
        public string Mobile { get; set; }

        //[NopResourceDisplayName("InjaKala.Account.PasswordRecovery.Email")]
        //public string Email { get; set; }

        [NopResourceDisplayName("mobileValidation.fields.codeValidation")]
        public string Token { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [NopResourceDisplayName("account.changepassword.fields.newpassword")]
        public string Password { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [NopResourceDisplayName("account.fields.confirmpassword")]
        public string NewPassword { get; set; }

        public string Result { get; set; }
    }
}