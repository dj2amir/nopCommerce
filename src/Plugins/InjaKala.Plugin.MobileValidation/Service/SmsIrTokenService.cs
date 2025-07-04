//using Nop.Core.Domain.Logging;
//using Nop.Core.Infrastructure;
//using Nop.Services.Logging;
//using InjaKalaIR.Plugin.MobileValidation.SmsInterface;
//using SmsIrRestful;
//using System;
//using System.Collections.Generic;

//namespace InjaKalaIR.Plugin.MobileValidation.Service
//{
//    internal class SmsIrTokenService : ISmsService
//    {
//        public string Name
//        {
//            get { return "سامانه پیام کوتاه ایده پردازان sms.ir - بر اساس token"; }
//        }
//        public string Id => "18";
//        //sample sms define paterncode_param=value&param2=value2
//        public bool SendSms(MobileValidationSettings setting, List<Models.Envelope> envelope)
//        {
//            bool flag = false;
//            try
//            {
//                var token = (new SmsIrRestful.Token()).GetToken(setting.SmsUsername, setting.SmsPassword);
//                SmsIrRestful.MessageSend msg = new SmsIrRestful.MessageSend();
//                foreach (var item in envelope)
//                {
//                    long mobile;
//                    if (long.TryParse(item.To, out mobile))
//                    {
//                        var tempId = int.Parse(item.Message.Split("_")[0]);
//                        string temp = item.Message.Split("_")[1];

//                        var parameterArray = new List<UltraFastParameters>() { };

//                        foreach (var item2 in temp.Split("&"))
//                        {
//                            var ii = item2.Split("=");
//                            parameterArray.Add(new UltraFastParameters()
//                            {
//                                Parameter = ii[0],
//                                ParameterValue = ii[1]
//                            });
//                        }
//                        var ultraFastSend = new UltraFastSend()
//                        {
//                            Mobile = mobile,
//                            TemplateId = tempId,
//                            ParameterArray = parameterArray.ToArray()
//                        };
//                        UltraFastSendRespone result = new UltraFast().Send(token, ultraFastSend);
//                        flag = result.IsSuccessful;
//                        if (!flag)
//                            EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Information, "SMS.IR SendSms ", result.VerificationCodeId.ToString(), null);
//                    }
//                    else
//                    {
//                        EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Information, "SMS.IR SendSms Phone number not valid" + item.To, "", null);
//                    }

//                    //var result=msg.Send(token,
//                    //new SmsIrRestful.MessageSendObject()
//                    //{
//                    //    MobileNumbers = new string[] { item.To },
//                    //    LineNumber = setting.SmsLineNumber,
//                    //    Messages = new string[] { item.Message },
//                    //    SendDateTime = null,
//                    //    CanContinueInCaseOfError = false
//                    //});
//                }
//            }
//            catch (Exception ex)
//            {
//                EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Error, "SMS.IR SendSms Error", ex.Message + " ", null);
//                bool result = flag;
//                return result;
//            }
//            return flag;
//        }

//        public int RemainingSms(MobileValidationSettings setting)
//        {
//            int num = -1;
//            try
//            {
//                var token = (new SmsIrRestful.Token()).GetToken(setting.SmsUsername, setting.SmsPassword);
//                var credit = (new SmsIrRestful.Credit()).GetCredit(token);
//                num = Convert.ToInt32(credit.Credit);
//            }
//            catch (Exception ex)
//            {
//                EngineContext.Current.Resolve<ILogger>().InsertLog(LogLevel.Error, "SMS.IR RemainingSms ", ex.Message, null);
//            }
//            return num;
//        }
//    }
//}