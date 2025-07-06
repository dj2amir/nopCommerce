using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.MultiFactorAuth.OTP.Models;

/// <summary>
/// Represents OTP verification model
/// </summary>
public record OTPVerificationModel : BaseNopModel
{
    #region Properties

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.OtpCode")]
    [Required]
    [StringLength(8, MinimumLength = 4)]
    public string OtpCode { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.BackupCode")]
    [StringLength(8, MinimumLength = 8)]
    public string BackupCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public bool CanUseBackupCode { get; set; }

    public bool IsBackupCodeUsed { get; set; }

    public string Instructions { get; set; } = string.Empty;

    public int RemainingAttempts { get; set; }

    #endregion
}