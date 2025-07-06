using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.OTP.Models;
using Nop.Plugin.MultiFactorAuth.OTP.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.MultiFactorAuth.OTP.Components;

/// <summary>
/// Represents OTP authentication view component
/// </summary>
public class OTPAuthenticationViewComponent : NopViewComponent
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IOtpService _otpService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public OTPAuthenticationViewComponent(ICustomerService customerService,
        ILocalizationService localizationService,
        IOtpService otpService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _localizationService = localizationService;
        _otpService = otpService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <param name="isConfiguringByCustomer">Whether customer is configuring the provider</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(bool isConfiguringByCustomer = false)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Content(""); // Return empty content for non-registered users

        var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);

        var model = new OTPAuthenticationModel
        {
            Instructions = await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.Instructions"),
            PhoneNumber = otpRecord?.PhoneNumber ?? string.Empty,
            IsPhoneNumberVerified = otpRecord?.IsPhoneNumberVerified ?? false,
            IsEnabled = otpRecord?.IsEnabled ?? false,
            HasBackupCodes = !string.IsNullOrEmpty(otpRecord?.BackupCodes),
            IsConfiguringByCustomer = isConfiguringByCustomer
        };

        return View("~/Plugins/MultiFactorAuth.OTP/Views/Customer/OTPAuthentication.cshtml", model);
    }

    #endregion
}