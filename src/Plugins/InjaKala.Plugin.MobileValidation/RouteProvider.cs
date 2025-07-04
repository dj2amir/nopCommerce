using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Mvc.Routing;

namespace InjaKalaIR.Plugin.MobileValidation
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("InjaKalaIR.Plugin.MobileValidation.Configure", "Admin/Plugin/InjaKalaIR.Plugin.MobileValidation/Sms/Configure/",
                new { controller = "InjaKalaCustomerValidation", action = "Configure" });

           
            var setting = EngineContext.Current.Resolve<ISettingService>().LoadSettingAsync<MobileValidationSettings>().Result;

            if (setting.SmsIsActive)
            {
                endpointRouteBuilder.MapControllerRoute("InjaKalaRegister11MV", "register/", defaults:
                new { controller = "InjaKalaCustomerValidation", action = "register" });

                endpointRouteBuilder.MapControllerRoute("InjaKalaRegister11MVfa", "fa/register/",
                    new { controller = "InjaKalaCustomerValidation", action = "register" });

                endpointRouteBuilder.MapControllerRoute("NFRegisterMV", "nfregister/",
                    new { controller = "InjaKalaCustomerValidation", action = "nfregister" });


                endpointRouteBuilder.MapControllerRoute("InjaKalaRegisterResultMV", "nfregisterresult/{resultId}",
                new { controller = "InjaKalaCustomerValidation", action = "InjaKalaRegisterResult" });

                endpointRouteBuilder.MapControllerRoute("InjaKalaRegistrationVerification", "RegistrationVerification",
                new { controller = "InjaKalaCustomer", action = "RegistrationVerification" });

                endpointRouteBuilder.MapControllerRoute("InjaKalapasswordrecovery", "passwordrecovery/",
                new { controller = "InjaKalaCustomerValidation", action = "NFPasswordRecovery" });

                endpointRouteBuilder.MapControllerRoute("InjaKalaOtp", "Otp/",
                    new { controller = "InjaKalaOtp", action = "index" });
                if (setting.SmsIsUseCustomLogin)
                {
                    endpointRouteBuilder.MapControllerRoute("InjaKalaMobilelogin", "login/",
                        new { controller = "InjaKalaCustomerValidation", action = "NFLogin" });
                }
            }

        }

        public static string Routes(HttpRequest Request)
        {
            
            return "";
            return "Routes";
        }

        public int Priority
        {
            get
            {
                return 11002;
            }
        }
    }
}