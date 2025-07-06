using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.OTP.Models;
using Nop.Plugin.MultiFactorAuth.OTP.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.MultiFactorAuth.OTP.Components;

/// <summary>
/// Represents OTP verification view component
/// </summary>
[ViewComponent(Name = "OTPVerification")]
public class OTPVerificationViewComponent : NopViewComponent
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IOtpService _otpService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public OTPVerificationViewComponent(ILocalizationService localizationService,
        IOtpService otpService,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _otpService = otpService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <returns>View component result</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);

        var model = new OTPVerificationModel
        {
            PhoneNumber = otpRecord?.PhoneNumber ?? string.Empty,
            CanUseBackupCode = !string.IsNullOrEmpty(otpRecord?.BackupCodes),
            Instructions = await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.VerificationInstructions"),
            RemainingAttempts = _otpService.HasRemainingAttempts(customer.Id) ? 3 : 0 // This could be improved to show actual remaining attempts
        };

        return View("~/Plugins/MultiFactorAuth.OTP/Views/Customer/OTPVerification.cshtml", model);
    }

    #endregion
}