using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.MultiFactorAuth.OTP.Domain;

namespace Nop.Plugin.MultiFactorAuth.OTP.Data;

[NopMigration("2024/01/01 12:00:00", "MultiFactorAuth.OTP base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<OTPRecord>();
    }
}