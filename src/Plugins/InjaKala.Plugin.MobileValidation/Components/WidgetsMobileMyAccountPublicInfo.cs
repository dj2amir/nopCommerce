using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Components
{
    public class WidgetsMobileMyAccountPublicInfoViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return base.View("~/Plugins/InjaKalaIR.Plugin.MobileValidation/Views/InjaKalaCustomerValidation/MobileMyAccountPublic.cshtml");
        }
    }
}