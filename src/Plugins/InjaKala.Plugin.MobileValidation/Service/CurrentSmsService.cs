using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Caching;
using Nop.Services.Configuration;
using InjaKalaIR.Plugin.MobileValidation.SmsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Service
{
    internal static class CurrentSmsService
    {
        public static ISmsService SmsService
        {
            get
            {
                return EngineContext.Current.Resolve<IStaticCacheManager>().Get<ISmsService>(new CacheKey("SMS_WebService"), () =>
                {
                    MobileValidationSettings MobileValidationSettings = EngineContext.Current.Resolve<ISettingService>().LoadSettingAsync<MobileValidationSettings>().Result;

                    var assemblyName = "InjaKalaIR.Plugin.MobileValidation";

                    var asm = Assembly.Load(assemblyName);

                    var type = typeof(ISmsService);
                    var types = //AppDomain.CurrentDomain.GetAssemblies()
                                //.SelectMany(s => s.GetTypes())
                        asm.GetTypes()
                        .Where(p => type.IsAssignableFrom(p) && p.IsClass);
                    foreach (var item in types)
                    {
                        var v = ((ISmsService)Activator.CreateInstance(item));
                        if (v.Id == MobileValidationSettings.WebServiceSupport)
                        {
                            //var cacheKeyService = EngineContext.Current.Resolve<ICacheKeyService>();
                            EngineContext.Current.Resolve<IStaticCacheManager>().SetAsync(new CacheKey("SMS_WebService") { CacheTime = int.MaxValue }, v).Wait();
                            return v;
                        }
                    }
                    return new KavenegarSmsService();
                });

                
            }
            set
            {
                SmsService = value;
            }
        }
    }
}