using InjaKalaIR.Plugin.MobileValidation.Models;
using System;
using System.Collections.Generic;

namespace InjaKalaIR.Plugin.MobileValidation.SmsInterface
{
    internal interface ISmsService
    {
        string Id { get; }
        string Name { get; }

        bool SendSms(MobileValidationSettings setting, List<Envelope> envelope);

        int RemainingSms(MobileValidationSettings setting);
    }
}