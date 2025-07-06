namespace Nop.Plugin.MultiFactorAuth.OTP.Services;

/// <summary>
/// Represents SMS service interface
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send OTP code via SMS
    /// </summary>
    /// <param name="phoneNumber">Phone number</param>
    /// <param name="otpCode">OTP code</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<bool> SendOtpAsync(string phoneNumber, string otpCode);

    /// <summary>
    /// Check if SMS service is configured
    /// </summary>
    /// <returns>True if configured, false otherwise</returns>
    bool IsConfigured();

    /// <summary>
    /// Get SMS service status
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<string> GetStatusAsync();
}