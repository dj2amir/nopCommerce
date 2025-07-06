using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.OTP.Domain;
using Nop.Plugin.MultiFactorAuth.OTP.Models;
using Nop.Plugin.MultiFactorAuth.OTP.Services;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.MultiFactorAuth.OTP.Controllers;

/// <summary>
/// Represents customer OTP controller
/// </summary>
[HttpsRequirement]
public class CustomerOTPController : BaseController
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IOtpService _otpService;
    private readonly IWorkContext _workContext;
    private readonly OTPSettings _otpSettings;

    #endregion

    #region Ctor

    public CustomerOTPController(ICustomerService customerService,
        IAuthenticationService authenticationService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IOtpService otpService,
        IWorkContext workContext,
        OTPSettings otpSettings)
    {
        _customerService = customerService;
        _authenticationService = authenticationService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _otpService = otpService;
        _workContext = workContext;
        _otpSettings = otpSettings;
    }

    #endregion

    #region Methods

    [HttpPost]
    public virtual async Task<IActionResult> SendOtp(string phoneNumber)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            if (string.IsNullOrEmpty(phoneNumber))
                return Json(new { success = false, message = "شماره تلفن الزامی است" });

            var result = await _otpService.SendOtpCodeAsync(customer.Id, phoneNumber);
            if (result)
            {
                return Json(new { success = true, message = "کد تایید ارسال شد" });
            }
            else
            {
                return Json(new { success = false, message = "خطا در ارسال کد تایید" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> VerifyOtp(string phoneNumber, string otpCode)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            if (string.IsNullOrEmpty(otpCode))
                return Json(new { success = false, message = "کد تایید الزامی است" });

            var result = await _otpService.VerifyOtpCodeAsync(customer.Id, otpCode);
            if (result)
            {
                // Update or create OTP record
                var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);
                if (otpRecord == null)
                {
                    otpRecord = new OTPRecord
                    {
                        CustomerId = customer.Id,
                        PhoneNumber = phoneNumber,
                        IsPhoneNumberVerified = true,
                        IsEnabled = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    await _otpService.InsertOtpRecordAsync(otpRecord);
                }
                else
                {
                    otpRecord.PhoneNumber = phoneNumber;
                    otpRecord.IsPhoneNumberVerified = true;
                    otpRecord.UpdatedOnUtc = DateTime.UtcNow;
                    await _otpService.UpdateOtpRecordAsync(otpRecord);
                }

                return Json(new { success = true, message = "شماره تلفن تایید شد" });
            }
            else
            {
                return Json(new { success = false, message = "کد تایید نامعتبر است" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> EnableOtp()
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);
            if (otpRecord == null || !otpRecord.IsPhoneNumberVerified)
                return Json(new { success = false, message = "ابتدا شماره تلفن را تایید کنید" });

            otpRecord.IsEnabled = true;
            otpRecord.UpdatedOnUtc = DateTime.UtcNow;
            await _otpService.UpdateOtpRecordAsync(otpRecord);

            return Json(new { success = true, message = "تایید دو مرحله‌ای فعال شد" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> DisableOtp()
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);
            if (otpRecord == null)
                return Json(new { success = false, message = "رکورد OTP یافت نشد" });

            otpRecord.IsEnabled = false;
            otpRecord.UpdatedOnUtc = DateTime.UtcNow;
            await _otpService.UpdateOtpRecordAsync(otpRecord);

            return Json(new { success = true, message = "تایید دو مرحله‌ای غیرفعال شد" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> GenerateBackupCodes()
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);
            if (otpRecord == null || !otpRecord.IsEnabled)
                return Json(new { success = false, message = "ابتدا OTP را فعال کنید" });

            var backupCodes = _otpService.GenerateBackupCodes();
            otpRecord.BackupCodes = string.Join(",", backupCodes);
            otpRecord.UsedBackupCodes = string.Empty;
            otpRecord.UpdatedOnUtc = DateTime.UtcNow;
            await _otpService.UpdateOtpRecordAsync(otpRecord);

            return Json(new { success = true, codes = backupCodes, message = "کدهای پشتیبان تولید شد" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetBackupCodes()
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);
            if (otpRecord == null || string.IsNullOrEmpty(otpRecord.BackupCodes))
                return Json(new { success = false, message = "کدهای پشتیبان یافت نشد" });

            var backupCodes = otpRecord.BackupCodes.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var usedBackupCodes = otpRecord.UsedBackupCodes?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            
            var availableCodes = backupCodes.Where(code => !usedBackupCodes.Contains(code)).ToList();

            return Json(new { success = true, codes = availableCodes });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> VerifyMultiFactorAuth(string otpCode, string backupCode, string provider)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            bool isValid = false;

            if (!string.IsNullOrEmpty(otpCode))
            {
                isValid = await _otpService.VerifyOtpCodeAsync(customer.Id, otpCode);
            }
            else if (!string.IsNullOrEmpty(backupCode))
            {
                isValid = await _otpService.VerifyBackupCodeAsync(customer.Id, backupCode);
            }

            if (isValid)
            {
                // Mark multi-factor authentication as completed
                // This would typically be handled by nopCommerce's multi-factor authentication system
                return Json(new { success = true, message = "تایید موفق", redirectUrl = "/" });
            }
            else
            {
                return Json(new { success = false, message = "کد تایید نامعتبر است" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ResendOtp()
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Json(new { success = false, message = "کاربر احراز هویت نشده است" });

            var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);
            if (otpRecord == null)
                return Json(new { success = false, message = "رکورد OTP یافت نشد" });

            var result = await _otpService.SendOtpCodeAsync(customer.Id, otpRecord.PhoneNumber);
            if (result)
            {
                return Json(new { success = true, message = "کد تایید مجدداً ارسال شد" });
            }
            else
            {
                return Json(new { success = false, message = "خطا در ارسال کد تایید" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    /// <summary>
    /// Configure OTP provider for customer
    /// </summary>
    /// <param name="providerSysName">Provider system name</param>
    /// <returns>Configuration view</returns>
    public virtual async Task<IActionResult> ConfigureProvider(string providerSysName = "MultiFactorAuth.OTP")
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null || !await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var otpRecord = await _otpService.GetOtpRecordAsync(customer.Id);

            var model = new OTPAuthenticationModel
            {
                Instructions = await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.OTP.Instructions"),
                PhoneNumber = otpRecord?.PhoneNumber ?? string.Empty,
                IsPhoneNumberVerified = otpRecord?.IsPhoneNumberVerified ?? false,
                IsEnabled = otpRecord?.IsEnabled ?? false,
                HasBackupCodes = !string.IsNullOrEmpty(otpRecord?.BackupCodes)
            };

            return View("~/Plugins/MultiFactorAuth.OTP/Views/Customer/OTPAuthentication.cshtml", model);
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"خطا: {ex.Message}");
            return RedirectToRoute("MultiFactorAuthenticationSettings");
        }
    }

    #endregion
}