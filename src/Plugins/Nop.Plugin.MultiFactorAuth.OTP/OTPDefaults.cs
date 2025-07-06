namespace Nop.Plugin.MultiFactorAuth.OTP;

/// <summary>
/// Represents constants for the OTP multi-factor authentication method
/// </summary>
public static class OTPDefaults
{
    /// <summary>
    /// Gets the OTP multi-factor authentication method system name
    /// </summary>
    public static string SystemName => "MultiFactorAuth.OTP";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.MultiFactorAuth.OTP.Configure";

    /// <summary>
    /// Gets the default OTP code length
    /// </summary>
    public static int DefaultOtpCodeLength => 6;

    /// <summary>
    /// Gets the default OTP expiration time in minutes
    /// </summary>
    public static int DefaultOtpExpirationMinutes => 5;

    /// <summary>
    /// Gets the default maximum OTP attempts
    /// </summary>
    public static int DefaultMaxOtpAttempts => 3;

    /// <summary>
    /// Gets the cache key for OTP codes
    /// </summary>
    public static string OtpCacheKey => "Nop.Plugin.MultiFactorAuth.OTP.{0}";

    /// <summary>
    /// Gets the cache key for OTP attempts
    /// </summary>
    public static string OtpAttemptsCacheKey => "Nop.Plugin.MultiFactorAuth.OTP.Attempts.{0}";

    /// <summary>
    /// Gets the cache expiration time for OTP codes
    /// </summary>
    public static TimeSpan OtpCacheExpiration => TimeSpan.FromMinutes(DefaultOtpExpirationMinutes);

    /// <summary>
    /// Gets the cache expiration time for OTP attempts
    /// </summary>
    public static TimeSpan OtpAttemptsCacheExpiration => TimeSpan.FromMinutes(30);
}