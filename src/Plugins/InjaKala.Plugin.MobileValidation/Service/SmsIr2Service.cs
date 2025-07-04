using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using InjaKalaIR.Plugin.MobileValidation.SmsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Service
{
    internal class SmsIrService : ISmsService
    {
        public string Name
        {
            get { return "ایده پردازان sms.ir"; }
        }

        public string Id => "38";

        private class tokenResult
        {
            public string TokenKey { get; set; }
            public bool IsSuccessful { get; set; }
        }

        public bool SendSms(MobileValidationSettings setting, List<Models.Envelope> envelope)
        {
            bool flag = false;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient client = new HttpClient(handler, false);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("UserApiKey", setting.SmsPassword),
                    new KeyValuePair<string, string>("SecretKey", setting.SmsUsername)
                });
                string url = string.Format("http://RestfulSms.com/api/Token");
                var responseString = client.PostAsync(url, content).Result;
                var tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<tokenResult>(responseString.Content.ReadAsStringAsync().Result);

                client.DefaultRequestHeaders.Add("x-sms-ir-secure-token", tokenResult.TokenKey);
                client.DefaultRequestHeaders.Authorization
                             = new AuthenticationHeaderValue("x-sms-ir-secure-token", tokenResult.TokenKey);

                foreach (var item in envelope)
                {
                    var pairs = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("LineNumber", setting.SmsLineNumber),
                        new KeyValuePair<string, string>("MobileNumbers", "[\""+item.To+"\"]"),
                        new KeyValuePair<string, string>("Messages", "[\""+item.Message+"\"]"),
                        new KeyValuePair<string, string>("CanContinueInCaseOfError", "[\"false\"]"),
                    };

                    var content234 = new FormUrlEncodedContent(pairs);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://RestfulSms.com/api/MessageSend");
                    request.Content = content234;
                    var res = client.SendAsync(request).Result;

                    if (res.Content.ReadAsStringAsync().Result.Contains("true"))
                        flag = true;
                }
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Error, "SMS.IR SendSms ", ex.Message, null);
                bool result = flag;
                return result;
            }
            return flag;
        }

        private class creditResult
        {
            public string credit { get; set; }
            public bool IsSuccessful { get; set; }
        }

        public int RemainingSms(MobileValidationSettings setting)
        {
            int num = -1;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient client = new HttpClient(handler, false);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("UserApiKey", setting.SmsPassword),
                    new KeyValuePair<string, string>("SecretKey", setting.SmsUsername)
                });
                string url = string.Format("http://RestfulSms.com/api/Token");
                var responseString = client.PostAsync(url, content).Result;
                var tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<tokenResult>(responseString.Content.ReadAsStringAsync().Result);

                client.DefaultRequestHeaders.Add("x-sms-ir-secure-token", tokenResult.TokenKey);
                client.DefaultRequestHeaders.Authorization
                             = new AuthenticationHeaderValue("x-sms-ir-secure-token", tokenResult.TokenKey);

                var res = Newtonsoft.Json.JsonConvert.DeserializeObject<creditResult>(client.GetAsync("http://RestfulSms.com/api/credit").Result.Content.ReadAsStringAsync().Result);
                return int.Parse(res.credit.Split('.')[0]);
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Error, "SMS.IR RemainingSms ", ex.Message, null);
            }
            return num;
        }
    }
}