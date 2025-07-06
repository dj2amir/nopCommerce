using Nop.Core;

namespace Nop.Plugin.MultiFactorAuth.OTP.Domain;

/// <summary>
/// Represents an OTP record for a customer
/// </summary>
public partial class OTPRecord : BaseEntity
{
    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the phone number is verified
    /// </summary>
    public bool IsPhoneNumberVerified { get; set; }

    /// <summary>
    /// Gets or sets the date when the record was created
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date when the record was last updated
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets whether the OTP is enabled for this customer
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the backup codes (comma-separated)
    /// </summary>
    public string BackupCodes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the used backup codes (comma-separated)
    /// </summary>
    public string UsedBackupCodes { get; set; } = string.Empty;
}