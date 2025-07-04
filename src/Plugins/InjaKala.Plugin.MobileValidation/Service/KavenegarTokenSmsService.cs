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
using System.Net.Http.Json;
using InjaKalaIR.Plugin.MobileValidation.Models;

namespace InjaKalaIR.Plugin.MobileValidation.Service
{
    internal class KavenegarTokenSmsService : ISmsService
    {
        //حتما داخل کد سرچ شود.
        public string Name
        {
            get { return "کاوه نگار - توکن"; }
        }

        public string Id => "70";

        //https://api.kavenegar.com/v1/34474448435A4B5167373149487A787A592F667A643838315A597949733655703775484F7A7577593932453D/verify/lookup.json?receptor=09903339525&token=852596&token2=852596&template=CustomerRegisterationPersian
        public bool SendSms(MobileValidationSettings setting, List<Envelope> envelope)
        {
            bool flag = false;

            try
            {
                foreach (var item in envelope)
                {
                    long[] recId = new long[0];
                    byte[] status = new byte[0];
                    string[] to = new string[] { item.To };
                    //var result = payamak.SendSimpleSMS2(setting.SmsUsername, setting.SmsPassword, item.To, setting.SmsLineNumber, item.Message, false);
                    HttpClient client = new HttpClient();

                    var mm = item.Message.Split('=');
                    string tokens = "";
                    if (mm.Count() > 1)
                    {
                        int i = 0;
                        if (!mm[0].StartsWith("v2_"))
                        {
                            foreach (var token in (mm[1].Split('&')))
                            {
                                tokens += "token" + (i == 0 ? "" : (i) + "") + "=" + token + "&";
                                i++;
                            }
                        }
                        else
                        {
                            tokens = mm[1].Replace("_", "=") + "&";
                        }
                        //foreach (var token in (mm[1].Split('&')))
                        //{
                        //    if (i == 30)
                        //        i = 1;
                        //    tokens += "token" + (i == 0 ? "" : (i + 1) + "") + "=" + token + "&";
                        //    if (mm[0].StartsWith("v2_") && !tokens.Contains("token10"))
                        //    {
                        //        i += 9;
                        //    }
                        //    else
                        //        i++;
                        //}

                        string url = string.Format(
                            "https://api.kavenegar.com/v1/{0}/verify/lookup.json?receptor={1}&{2}template={3}",
                            setting.SmsUsername, item.To, tokens, mm[0].Replace("v2_", "")
                            );

                        //EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Information, "KavenegarTokenSmsService : " + url).Wait();

                        var responseString = client.GetStringAsync(url).Result;
                        flag = responseString.Contains("\"status\":200");
                    }
                    else
                    {
                        // todo : پیاده سازی به صورت ارسال معمولی
                        EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Information, "KavenegarTokenSmsService : Send simple not implement").Wait();
                        string url = string.Format(
                            "https://api.kavenegar.com/v1/{0}/sms/send.json?receptor={1}&message={2}&sender={3}",
                            setting.SmsUsername, item.To, item.Message, setting.SmsLineNumber
                            );

                        //EngineContext.Current.Resolve<ILogger>().WarningAsync("C5" + url).Wait();
                        var responseString = client.GetStringAsync(url).Result;
                        //EngineContext.Current.Resolve<ILogger>().Warning("C6" + responseString + url);
                        flag = responseString.Contains("\"status\":200");
                    }
                    //EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Information, "SendSms01 :" + result, "SendSms01 :" + result, null);
                }

                //return true;
                //Messenger messenger = new Messenger(setting.SmsUsername, setting.SmsPassword);
                //if (messenger != null)
                //{
                //    Header expr_1F = new Header();
                //    expr_1F.set_From(setting.SmsSubject);
                //    expr_1F.set_ValidityPeriod(1440);
                //    Header header = expr_1F;
                //    SubmitMultiResponse submitMultiResponse = messenger.SubmitMulti(envelope, header, 0);
                //    if (submitMultiResponse != null && submitMultiResponse.get_Response() != null && submitMultiResponse.get_Response().get_Status().get_Code() == 200)
                //    {
                //        flag = true;
                //    }
                //    else
                //    {
                //        bool result;
                //        if (submitMultiResponse.get_Response().get_Status().get_Code() != 200)
                //        {
                //            EngineContext.get_Current().Resolve<ILogger>().InsertLog(40, "SendSms01 ", submitMultiResponse.get_Response().get_Status().get_Code().ToString(), null);
                //            result = flag;
                //            return result;
                //        }
                //        EngineContext.get_Current().Resolve<ILogger>().InsertLog(40, "SendSms02 ", "Code:" + submitMultiResponse.get_Response().get_Status().get_Code().ToString() + " Description:" + submitMultiResponse.get_Response().get_Status().get_Description().ToString(), null);
                //        result = flag;
                //        return result;
                //    }
                //}
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().ErrorAsync("SMS kave token error ", ex).Wait();
                bool result = flag;
                return result;
            }
            
            return flag;
        }

        public int RemainingSms(MobileValidationSettings setting)
        {
            HttpClient client = new HttpClient();
            string url = string.Format(
                            "https://api.kavenegar.com/v1/{0}/account/info.json",
                            setting.SmsUsername);

            var responseString = client.GetStringAsync(url).Result;

            var json = System.Text.Json.JsonSerializer.Deserialize<ResponseObj>(responseString);

            return json.entries.remaincredit;


        }

        public class ResponseObj
        {
            public object @return { get; set; }
            public entriesObj entries { get; set; }
            public class entriesObj
            {
                public int remaincredit { get; set; }
                public long expiredate { get; set; }
            }
        }

    }
}