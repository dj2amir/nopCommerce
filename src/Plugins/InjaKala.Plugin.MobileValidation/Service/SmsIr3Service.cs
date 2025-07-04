using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using InjaKalaIR.Plugin.MobileValidation;
using InjaKalaIR.Plugin.MobileValidation.Models;
using InjaKalaIR.Plugin.MobileValidation.SmsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.SMS.Service
{
    internal class SmsIr3Service : ISmsService
    {
        public string Name
        {
            get { return "ایده پردازان sms.ir API V1"; }
        }

        public string Id => "74";

        private class tokenResult
        {
            public string TokenKey { get; set; }
            public bool IsSuccessful { get; set; }
        }

        public bool SendSms(MobileValidationSettings setting, List<Envelope> envelope)
        {
            bool flag = false;
            try
            {
                foreach (var item in envelope)
                {
                    var _httpClient = new HttpClient
                    {
                        BaseAddress = new Uri("https://api.sms.ir/v1/")
                    };

                    var verify = new List<VerifySendParameter>();
                    var param = item.Message.Split("_")[1].Split(',');
                    foreach (var item2 in param)
                    {
                        verify.Add(new VerifySendParameter(item2.Split("=")[0], item2.Split("=")[1]));
                    }

                    _httpClient.DefaultRequestHeaders.Add("x-api-key", setting.SmsUsername);
                    VerifySendRequest verifySendRequest = new VerifySendRequest(item.To, int.Parse(item.Message.Split("_")[0]), verify.ToArray());

                    // Log the message before sending
                    EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Information, "SMS.IR SendSms: Sending message", $"To: {item.To}, TemplateId: {verifySendRequest.TemplateId}, Parameters: {string.Join(", ", verify.Select(v => $"{v.Name}={v.Value}"))}", null).Wait();

                    var result = _httpClient.PostAsJsonAsync("send/verify", verifySendRequest).Result;

                    // Log the result of sending
                    EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Error, "SMS.IR SendSms: ", result.Content.ReadAsStringAsync().Result, null).Wait();
                }
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Error, "SMS.IR SendSms " + ex.Message, ex.InnerException?.Message).Wait();
                return flag;
            }
            return flag;
        }

        public class VerifySendResult
        {
            public int MessageId { get; set; }
            public decimal Cost { get; set; }
        }

        internal class VerifySendRequest
        {
            public VerifySendRequest(string mobile, int templateId, VerifySendParameter[] parameters)
            {
                Mobile = mobile;
                TemplateId = templateId;
                Parameters = parameters;
            }

            public string Mobile { get; set; }

            public int TemplateId { get; set; }

            public VerifySendParameter[] Parameters { get; set; }
        }

        public class VerifySendParameter
        {
            public VerifySendParameter(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }
            public string Value { get; set; }
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("x-sms-ir-secure-token", tokenResult.TokenKey);

                var res = Newtonsoft.Json.JsonConvert.DeserializeObject<creditResult>(client.GetAsync("http://RestfulSms.com/api/credit").Result.Content.ReadAsStringAsync().Result);
                return int.Parse(res.credit.Split('.')[0]);
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Error, "SMS.IR RemainingSms ", ex.Message, null).Wait();
            }
            return num;
        }
    }
}
