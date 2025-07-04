using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Models
{
    public class RecoveryPasswordValidationPhone
    {
        public string ConfirmCode { get; set; }
        public string NewPass { get; set; }
        public string ConfirmNewPass { get; set; }
    }
}