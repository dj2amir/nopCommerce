using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using FluentValidation;

namespace Nop.Plugin.MultiFactorAuth.OTP.Models;

/// <summary>
/// Represents configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Properties

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.SmsProviderApiKey")]
    [Required]
    public string SmsProviderApiKey { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.SmsProviderSecretKey")]
    [Required]
    public string SmsProviderSecretKey { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.SmsProviderBaseUrl")]
    [Required]
    public string SmsProviderBaseUrl { get; set; } = "https://api.sms.ir";

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.SmsTemplateId")]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "شناسه قالب پیامک باید بزرگتر از 0 باشد")]
    public int SmsTemplateId { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.SenderNumber")]
    public string SenderNumber { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.OtpCodeLength")]
    [Range(4, 8, ErrorMessage = "طول کد OTP باید بین 4 تا 8 رقم باشد")]
    public int OtpCodeLength { get; set; } = 6;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.OtpExpirationMinutes")]
    [Range(1, 30, ErrorMessage = "مدت انقضا باید بین 1 تا 30 دقیقه باشد")]
    public int OtpExpirationMinutes { get; set; } = 5;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.MaxOtpAttempts")]
    [Range(1, 10, ErrorMessage = "حداکثر تلاش باید بین 1 تا 10 باشد")]
    public int MaxOtpAttempts { get; set; } = 3;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.EnableForLogin")]
    public bool EnableForLogin { get; set; } = true;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.EnableForRegistration")]
    public bool EnableForRegistration { get; set; } = false;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.TestPhoneNumber")]
    public string TestPhoneNumber { get; set; } = string.Empty;

    public string TestResult { get; set; } = string.Empty;

    #endregion
}

/// <summary>
/// Represents validator for configuration model
/// </summary>
public class ConfigurationModelValidator : AbstractValidator<ConfigurationModel>
{
    public ConfigurationModelValidator()
    {
        RuleFor(x => x.SmsProviderApiKey)
            .NotEmpty()
            .WithMessage("کلید API سرویس پیامک الزامی است")
            .MinimumLength(10)
            .WithMessage("کلید API باید حداقل 10 کاراکتر باشد");

        RuleFor(x => x.SmsProviderSecretKey)
            .NotEmpty()
            .WithMessage("کلید مخفی سرویس پیامک الزامی است")
            .MinimumLength(10)
            .WithMessage("کلید مخفی باید حداقل 10 کاراکتر باشد");

        RuleFor(x => x.SmsProviderBaseUrl)
            .NotEmpty()
            .WithMessage("آدرس پایه سرویس پیامک الزامی است")
            .Must(BeValidUrl)
            .WithMessage("آدرس پایه باید یک URL معتبر باشد");

        RuleFor(x => x.SmsTemplateId)
            .GreaterThan(0)
            .WithMessage("شناسه قالب پیامک الزامی است");

        RuleFor(x => x.OtpCodeLength)
            .InclusiveBetween(4, 8)
            .WithMessage("طول کد OTP باید بین 4 تا 8 رقم باشد");

        RuleFor(x => x.OtpExpirationMinutes)
            .InclusiveBetween(1, 30)
            .WithMessage("مدت انقضا باید بین 1 تا 30 دقیقه باشد");

        RuleFor(x => x.MaxOtpAttempts)
            .InclusiveBetween(1, 10)
            .WithMessage("حداکثر تلاش باید بین 1 تا 10 باشد");

        RuleFor(x => x.TestPhoneNumber)
            .Matches(@"^09\d{9}$")
            .When(x => !string.IsNullOrEmpty(x.TestPhoneNumber))
            .WithMessage("شماره تلفن باید با فرمت صحیح (09xxxxxxxxx) وارد شود");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) && 
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}