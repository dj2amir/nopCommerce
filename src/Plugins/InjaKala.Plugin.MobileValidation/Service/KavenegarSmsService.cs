using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using InjaKalaIR.Plugin.MobileValidation.SmsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Service
{
    internal class KavenegarSmsService : ISmsService
    {
        public string Name
        {
            get { return "سامانه پیامک کاوه نگار"; }
        }

        public string Id => "23";

        public bool SendSms(MobileValidationSettings setting, List<Models.Envelope> envelope)
        {
            bool flag = false;
            try
            {
                HttpClient client = new HttpClient();
                foreach (var item in envelope)
                {
                    string url = string.Format(
                        "https://api.kavenegar.com/v1/{0}/sms/send.json?receptor={1}&message={2}&sender={3}", setting.SmsUsername, item.To, item.Message, setting.ManagerPhoneNumber);
                    var responseString = client.GetStringAsync(url).Result;
                    if (responseString.Contains("status\":1"))
                        flag = true;
                }
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Error, "Kavenegar SendSms ", ex.Message, null);
                bool result = flag;
                return result;
            }
            return flag;
        }

        public int RemainingSms(MobileValidationSettings setting)
        {
            int num = -1;
            try
            {
                HttpClient client = new HttpClient();
                string url = string.Format(
                        "https://api.kavenegar.com/v1/{0}/account/info.json", setting.SmsUsername);

                //EngineContext.Current.Resolve<ILogger>().Warning("C5" + url);

                var responseString = client.GetStringAsync(
                    url
                    ).Result;
                var str = responseString.Split("remaincredit\":")[1].Split(',')[0];
                return int.Parse(str);
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Error, "Kavenegar RemainingSms ", ex.Message, null);
            }
            return num;
        }
    }
}