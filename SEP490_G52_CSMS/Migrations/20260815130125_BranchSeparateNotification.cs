using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class BranchSeparateNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[orders]') AND name = N'bank_transaction_code')
                BEGIN
                    ALTER TABLE [orders] ADD [bank_transaction_code] nvarchar(100) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[orders]') AND name = N'refund_amount')
                BEGIN
                    ALTER TABLE [orders] ADD [refund_amount] decimal(18,2) NOT NULL DEFAULT 0;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[orders]') AND name = N'refund_method')
                BEGIN
                    ALTER TABLE [orders] ADD [refund_method] nvarchar(50) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[orders]') AND name = N'refund_reason')
                BEGIN
                    ALTER TABLE [orders] ADD [refund_reason] nvarchar(255) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[orders]') AND name = N'refunded_at')
                BEGIN
                    ALTER TABLE [orders] ADD [refunded_at] datetime2 NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[notifications]') AND name = N'branch_id')
                BEGIN
                    ALTER TABLE [notifications] ADD [branch_id] nvarchar(10) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[cash_handovers]') AND name = N'cash_refund_amount')
                BEGIN
                    ALTER TABLE [cash_handovers] ADD [cash_refund_amount] decimal(18,2) NOT NULL DEFAULT 0;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[cash_handovers]') AND name = N'emergency_reason')
                BEGIN
                    ALTER TABLE [cash_handovers] ADD [emergency_reason] nvarchar(255) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[cash_handovers]') AND name = N'handover_type')
                BEGIN
                    ALTER TABLE [cash_handovers] ADD [handover_type] nvarchar(50) NOT NULL DEFAULT '';
                END

                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'branch_settings')
                BEGIN
                    CREATE TABLE [branch_settings] (
                        [setting_id] int NOT NULL IDENTITY,
                        [branch_id] nvarchar(50) NOT NULL,
                        [branch_display_name] nvarchar(100) NOT NULL,
                        [contact_phone] nvarchar(20) NOT NULL,
                        [address] nvarchar(255) NOT NULL,
                        [opening_hours] nvarchar(100) NOT NULL,
                        [auto_print_receipt] bit NOT NULL,
                        [enable_sound_notification] bit NOT NULL,
                        [bank_code] nvarchar(20) NOT NULL,
                        [account_number] nvarchar(50) NOT NULL,
                        [account_name] nvarchar(100) NOT NULL,
                        [sepay_api_key] nvarchar(200) NULL,
                        [webhook_secret_token] nvarchar(200) NULL,
                        [transfer_prefix] nvarchar(50) NOT NULL,
                        [auto_confirm_order] bit NOT NULL,
                        [is_sepay_active] bit NOT NULL,
                        [updated_at] datetime2 NOT NULL,
                        CONSTRAINT [PK_branch_settings] PRIMARY KEY ([setting_id]),
                        CONSTRAINT [FK_branch_settings_branches_branch_id] FOREIGN KEY ([branch_id]) REFERENCES [branches] ([branch_id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_branch_settings_branch_id] ON [branch_settings] ([branch_id]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch_settings");

            migrationBuilder.DropColumn(
                name: "bank_transaction_code",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "refund_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "refund_method",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "refund_reason",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "refunded_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "cash_refund_amount",
                table: "cash_handovers");

            migrationBuilder.DropColumn(
                name: "emergency_reason",
                table: "cash_handovers");

            migrationBuilder.DropColumn(
                name: "handover_type",
                table: "cash_handovers");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
