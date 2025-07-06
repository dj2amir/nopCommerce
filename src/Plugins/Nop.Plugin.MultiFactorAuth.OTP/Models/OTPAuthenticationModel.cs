using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.MultiFactorAuth.OTP.Models;

/// <summary>
/// Represents OTP authentication model
/// </summary>
public record OTPAuthenticationModel : BaseNopModel
{
    #region Properties

    /// <summary>
    /// Instructions text
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.PhoneNumber")]
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Is phone number verified
    /// </summary>
    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.IsPhoneNumberVerified")]
    public bool IsPhoneNumberVerified { get; set; }

    /// <summary>
    /// Is OTP enabled
    /// </summary>
    [NopResourceDisplayName("Plugins.MultiFactorAuth.OTP.Fields.IsEnabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Has backup codes
    /// </summary>
    public bool HasBackupCodes { get; set; }

    /// <summary>
    /// Whether customer is configuring the provider
    /// </summary>
    public bool IsConfiguringByCustomer { get; set; }

    /// <summary>
    /// Result message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    #endregion
}