using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.MultiFactorAuth.OTP.Domain;

namespace Nop.Plugin.MultiFactorAuth.OTP.Data;

/// <summary>
/// Represents a OTP record entity builder
/// </summary>
public partial class OTPRecordBuilder : NopEntityBuilder<OTPRecord>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(OTPRecord.CustomerId)).AsInt32().NotNullable()
            .WithColumn(nameof(OTPRecord.PhoneNumber)).AsString(20).NotNullable()
            .WithColumn(nameof(OTPRecord.IsPhoneNumberVerified)).AsBoolean().NotNullable()
            .WithColumn(nameof(OTPRecord.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(OTPRecord.UpdatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(OTPRecord.IsEnabled)).AsBoolean().NotNullable()
            .WithColumn(nameof(OTPRecord.BackupCodes)).AsString(500).Nullable()
            .WithColumn(nameof(OTPRecord.UsedBackupCodes)).AsString(500).Nullable();
    }

    #endregion
}