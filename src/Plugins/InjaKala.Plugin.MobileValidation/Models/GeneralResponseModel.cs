using System;
using System.Collections.Generic;
using System.Text;

namespace InjaKalaIR.Plugin.MobileValidation.Models
{
    public class GeneralResponseModel<TResult>// : GeneralResponseModel //where TResult : new()
    {
        public TResult Data { get; set; }

        public GeneralResponseModel()
        {
            Data = Activator.CreateInstance<TResult>();
            StatusCode = (int)ErrorType.Ok;
            ErrorList = new List<string>();
            Errors = new List<string>();
            //SuccessMessage = StatusCode == (int)ErrorType.Ok ? "دریافت اطلاعات با موفقیت انجام شد" : StatusCode == (int)ErrorType.NotOk ? "خطا! دریافت اطلاعات ناموفق" : "خطای احراز هویت";
        }

        public string SuccessMessage { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public List<string> ErrorList { get; set; }
        public List<string> Errors { get; set; }
        public string Error { get; set; }
    }

    public enum ErrorType
    {
        Ok = 200,
        NotOk = 400,
        AuthenticationError = 600,
    }
}