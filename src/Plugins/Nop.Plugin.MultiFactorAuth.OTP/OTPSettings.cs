using Nop.Core.Configuration;

namespace Nop.Plugin.MultiFactorAuth.OTP;

/// <summary>
/// Represents settings of the OTP multi-factor authentication method
/// </summary>
public class OTPSettings : ISettings
{
    /// <summary>
    /// Gets or sets the SMS provider API key
    /// </summary>
    public string SmsProviderApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMS provider secret key
    /// </summary>
    public string SmsProviderSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMS provider base URL
    /// </summary>
    public string SmsProviderBaseUrl { get; set; } = "https://api.sms.ir";

    /// <summary>
    /// Gets or sets the SMS template ID
    /// </summary>
    public int SmsTemplateId { get; set; } = 0;

    /// <summary>
    /// Gets or sets the OTP code length
    /// </summary>
    public int OtpCodeLength { get; set; } = 6;

    /// <summary>
    /// Gets or sets the OTP code expiration time in minutes
    /// </summary>
    public int OtpExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of OTP attempts
    /// </summary>
    public int MaxOtpAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to enable OTP for login
    /// </summary>
    public bool EnableForLogin { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable OTP for registration
    /// </summary>
    public bool EnableForRegistration { get; set; } = false;

    /// <summary>
    /// Gets or sets the sender number for SMS
    /// </summary>
    public string SenderNumber { get; set; } = string.Empty;
}