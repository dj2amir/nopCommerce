using ir.sms1000;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using InjaKalaIR.Plugin.MobileValidation.SmsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Service
{
    internal class Sms1000SmsService : ISmsService
    {
        
        public string Name
        {
            get { return "سامانه sms1000 رهیاب گستران"; }
        }

        public string Id => "37";

        public bool SendSms(MobileValidationSettings setting, List<Models.Envelope> envelope)
        {
            var saba = new smsproSoapClient(smsproSoapClient.EndpointConfiguration.smsproSoap);
            bool flag = false;
            try
            {
                foreach (var item in envelope)
                {
                    long[] recId = new long[0];
                    byte[] status = new byte[0];
                    //string[] to = new string[] { item.To };
                    var result = saba.doSendSMSAsync(setting.SmsUsername, setting.SmsPassword, setting.SmsLineNumber, item.To, item.Message, true, false, false).Result.Body.doSendSMSResult;
                    EngineContext.Current.Resolve<ILogger>().InformationAsync("sms1000 SendSms :" + result).Wait();
                    if (result.Contains("Send OK"))
                        flag = true;
                }

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
                EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Error, "sms1000 SendSms ", ex.Message, null).Wait();
                bool result = flag;
                return result;
            }
            return flag;
        }

        public int RemainingSms(MobileValidationSettings setting)
        {
            int num = -1;
            var saba = new smsproSoapClient(smsproSoapClient.EndpointConfiguration.smsproSoap);
            //var result = saba.SendSms(setting.SmsUsername, setting.SmsPassword, setting.SmsLineNumber, to, item.Message, false, "", ref status, ref recId);
            try
            {
                long credit = 0;
                string expire = "";
                var result = saba.getInfoAsync(setting.SmsUsername, setting.SmsPassword, credit, expire).Result.Body;
                credit = result.uCredit;
                num = (int)credit;
            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().InsertLogAsync(LogLevel.Error, "RemainingSms sms1000 ", ex.Message, null).Wait();
            }
            return num;
        }
    }
}