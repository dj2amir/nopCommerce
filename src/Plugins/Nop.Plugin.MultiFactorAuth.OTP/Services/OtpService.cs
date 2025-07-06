using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.MultiFactorAuth.OTP.Domain;

namespace Nop.Plugin.MultiFactorAuth.OTP.Services;

/// <summary>
/// Represents OTP service implementation
/// </summary>
public class OtpService : IOtpService
{
    #region Fields

    private readonly OTPSettings _otpSettings;
    private readonly ISmsService _smsService;
    private readonly IRepository<OTPRecord> _otpRecordRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<OtpService> _logger;

    #endregion

    #region Ctor

    public OtpService(OTPSettings otpSettings,
        ISmsService smsService,
        IRepository<OTPRecord> otpRecordRepository,
        IMemoryCache memoryCache,
        ILogger<OtpService> logger)
    {
        _otpSettings = otpSettings;
        _smsService = smsService;
        _otpRecordRepository = otpRecordRepository;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Generate OTP code
    /// </summary>
    /// <returns>OTP code</returns>
    public string GenerateOtpCode()
    {
        var random = new Random();
        var code = new StringBuilder();
        
        for (int i = 0; i < _otpSettings.OtpCodeLength; i++)
        {
            code.Append(random.Next(0, 10));
        }
        
        return code.ToString();
    }

    /// <summary>
    /// Send OTP code to customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="phoneNumber">Phone number</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<bool> SendOtpCodeAsync(int customerId, string phoneNumber)
    {
        try
        {
            if (!HasRemainingAttempts(customerId))
            {
                _logger.LogWarning("Customer {CustomerId} has exceeded maximum OTP attempts", customerId);
                return false;
            }

            var otpCode = GenerateOtpCode();
            
            // Store OTP code in cache
            var cacheKey = string.Format(OTPDefaults.OtpCacheKey, customerId);
            var cacheExpiration = TimeSpan.FromMinutes(_otpSettings.OtpExpirationMinutes);
            _memoryCache.Set(cacheKey, otpCode, cacheExpiration);

            // Send SMS
            var result = await _smsService.SendOtpAsync(phoneNumber, otpCode);
            
            if (result)
            {
                _logger.LogInformation("OTP code sent successfully to customer {CustomerId}", customerId);
                return true;
            }
            else
            {
                // Remove from cache if SMS sending failed
                _memoryCache.Remove(cacheKey);
                _logger.LogWarning("Failed to send OTP code to customer {CustomerId}", customerId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP code to customer {CustomerId}", customerId);
            return false;
        }
    }

    /// <summary>
    /// Verify OTP code
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="otpCode">OTP code</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<bool> VerifyOtpCodeAsync(int customerId, string otpCode)
    {
        try
        {
            if (!HasRemainingAttempts(customerId))
            {
                _logger.LogWarning("Customer {CustomerId} has exceeded maximum OTP attempts", customerId);
                return false;
            }

            var cacheKey = string.Format(OTPDefaults.OtpCacheKey, customerId);
            
            if (_memoryCache.TryGetValue(cacheKey, out var cachedCode))
            {
                if (cachedCode.ToString() == otpCode)
                {
                    // Valid OTP code
                    _memoryCache.Remove(cacheKey);
                    ResetAttempts(customerId);
                    _logger.LogInformation("OTP code verified successfully for customer {CustomerId}", customerId);
                    return true;
                }
            }

            // Invalid OTP code
            IncrementAttempts(customerId);
            _logger.LogWarning("Invalid OTP code for customer {CustomerId}", customerId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP code for customer {CustomerId}", customerId);
            return false;
        }
    }

    /// <summary>
    /// Get OTP record for customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<OTPRecord> GetOtpRecordAsync(int customerId)
    {
        return await Task.FromResult(_otpRecordRepository.Table
            .FirstOrDefault(x => x.CustomerId == customerId));
    }

    /// <summary>
    /// Insert OTP record
    /// </summary>
    /// <param name="otpRecord">OTP record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task InsertOtpRecordAsync(OTPRecord otpRecord)
    {
        await _otpRecordRepository.InsertAsync(otpRecord);
    }

    /// <summary>
    /// Update OTP record
    /// </summary>
    /// <param name="otpRecord">OTP record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task UpdateOtpRecordAsync(OTPRecord otpRecord)
    {
        await _otpRecordRepository.UpdateAsync(otpRecord);
    }

    /// <summary>
    /// Delete OTP record
    /// </summary>
    /// <param name="otpRecord">OTP record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DeleteOtpRecordAsync(OTPRecord otpRecord)
    {
        await _otpRecordRepository.DeleteAsync(otpRecord);
    }

    /// <summary>
    /// Generate backup codes
    /// </summary>
    /// <returns>List of backup codes</returns>
    public List<string> GenerateBackupCodes()
    {
        var codes = new List<string>();
        var random = new Random();
        
        for (int i = 0; i < 10; i++)
        {
            var code = new StringBuilder();
            for (int j = 0; j < 8; j++)
            {
                code.Append(random.Next(0, 10));
            }
            codes.Add(code.ToString());
        }
        
        return codes;
    }

    /// <summary>
    /// Verify backup code
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="backupCode">Backup code</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<bool> VerifyBackupCodeAsync(int customerId, string backupCode)
    {
        try
        {
            var otpRecord = await GetOtpRecordAsync(customerId);
            if (otpRecord == null)
                return false;

            var backupCodes = otpRecord.BackupCodes?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            var usedBackupCodes = otpRecord.UsedBackupCodes?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            if (backupCodes.Contains(backupCode) && !usedBackupCodes.Contains(backupCode))
            {
                // Mark backup code as used
                otpRecord.UsedBackupCodes = string.IsNullOrEmpty(otpRecord.UsedBackupCodes)
                    ? backupCode
                    : $"{otpRecord.UsedBackupCodes},{backupCode}";
                
                await UpdateOtpRecordAsync(otpRecord);
                
                _logger.LogInformation("Backup code verified successfully for customer {CustomerId}", customerId);
                return true;
            }

            _logger.LogWarning("Invalid backup code for customer {CustomerId}", customerId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying backup code for customer {CustomerId}", customerId);
            return false;
        }
    }

    /// <summary>
    /// Check if customer has remaining OTP attempts
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>True if has remaining attempts, false otherwise</returns>
    public bool HasRemainingAttempts(int customerId)
    {
        var cacheKey = string.Format(OTPDefaults.OtpAttemptsCacheKey, customerId);
        
        if (_memoryCache.TryGetValue(cacheKey, out var attempts))
        {
            return (int)attempts < _otpSettings.MaxOtpAttempts;
        }
        
        return true;
    }

    /// <summary>
    /// Increment OTP attempts for customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    public void IncrementAttempts(int customerId)
    {
        var cacheKey = string.Format(OTPDefaults.OtpAttemptsCacheKey, customerId);
        var currentAttempts = 0;
        
        if (_memoryCache.TryGetValue(cacheKey, out var attempts))
        {
            currentAttempts = (int)attempts;
        }
        
        _memoryCache.Set(cacheKey, currentAttempts + 1, OTPDefaults.OtpAttemptsCacheExpiration);
    }

    /// <summary>
    /// Reset OTP attempts for customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    public void ResetAttempts(int customerId)
    {
        var cacheKey = string.Format(OTPDefaults.OtpAttemptsCacheKey, customerId);
        _memoryCache.Remove(cacheKey);
    }

    #endregion
}