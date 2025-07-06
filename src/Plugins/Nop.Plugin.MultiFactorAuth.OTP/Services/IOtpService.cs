using Nop.Plugin.MultiFactorAuth.OTP.Domain;

namespace Nop.Plugin.MultiFactorAuth.OTP.Services;

/// <summary>
/// Represents OTP service interface
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generate OTP code
    /// </summary>
    /// <returns>OTP code</returns>
    string GenerateOtpCode();

    /// <summary>
    /// Send OTP code to customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="phoneNumber">Phone number</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<bool> SendOtpCodeAsync(int customerId, string phoneNumber);

    /// <summary>
    /// Verify OTP code
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="otpCode">OTP code</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<bool> VerifyOtpCodeAsync(int customerId, string otpCode);

    /// <summary>
    /// Get OTP record for customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<OTPRecord> GetOtpRecordAsync(int customerId);

    /// <summary>
    /// Insert OTP record
    /// </summary>
    /// <param name="otpRecord">OTP record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertOtpRecordAsync(OTPRecord otpRecord);

    /// <summary>
    /// Update OTP record
    /// </summary>
    /// <param name="otpRecord">OTP record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateOtpRecordAsync(OTPRecord otpRecord);

    /// <summary>
    /// Delete OTP record
    /// </summary>
    /// <param name="otpRecord">OTP record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteOtpRecordAsync(OTPRecord otpRecord);

    /// <summary>
    /// Generate backup codes
    /// </summary>
    /// <returns>List of backup codes</returns>
    List<string> GenerateBackupCodes();

    /// <summary>
    /// Verify backup code
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="backupCode">Backup code</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<bool> VerifyBackupCodeAsync(int customerId, string backupCode);

    /// <summary>
    /// Check if customer has remaining OTP attempts
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>True if has remaining attempts, false otherwise</returns>
    bool HasRemainingAttempts(int customerId);

    /// <summary>
    /// Increment OTP attempts for customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    void IncrementAttempts(int customerId);

    /// <summary>
    /// Reset OTP attempts for customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    void ResetAttempts(int customerId);
}