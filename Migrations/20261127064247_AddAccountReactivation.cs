using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonBite.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountReactivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAccounts_Role_AccountType",
                table: "UserAccounts");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "VerificationCodes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Registration");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "UserAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeactivationReason",
                table: "UserAccounts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_UserId_Purpose_CreatedAt",
                table: "VerificationCodes",
                columns: new[] { "UserId", "Purpose", "CreatedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_VerificationCodes_Purpose",
                table: "VerificationCodes",
                sql: "[Purpose] IN ('Registration', 'Reactivation')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAccounts_DeactivationStatus",
                table: "UserAccounts",
                sql: "(\r\n            [IsActive] = 1\r\n            AND [DeactivationReason] IS NULL\r\n            AND [DeactivatedAt] IS NULL\r\n        )\r\n        OR\r\n        (\r\n            [IsActive] = 0\r\n            AND\r\n            (\r\n                (\r\n                    [DeactivationReason] IS NULL\r\n                    AND [DeactivatedAt] IS NULL\r\n                )\r\n                OR\r\n                (\r\n                    [DeactivationReason] IS NOT NULL\r\n                    AND [DeactivationReason] IN\r\n                        ('SelfDeactivated', 'AdminSuspended')\r\n                    AND [DeactivatedAt] IS NOT NULL\r\n                )\r\n            )\r\n        )");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAccounts_Role_AccountType",
                table: "UserAccounts",
                sql: "(\r\n            [Role] = 'Customer'\r\n            AND [AccountType] IS NOT NULL\r\n            AND [AccountType] IN\r\n                ('LocalCustomer', 'ForeignTourist')\r\n        )\r\n        OR\r\n        (\r\n            [Role] IN ('Admin', 'KitchenStaff', 'Shipper')\r\n            AND [AccountType] IS NULL\r\n            AND [Username] IS NOT NULL\r\n        )");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VerificationCodes_UserId_Purpose_CreatedAt",
                table: "VerificationCodes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VerificationCodes_Purpose",
                table: "VerificationCodes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAccounts_DeactivationStatus",
                table: "UserAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAccounts_Role_AccountType",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "DeactivationReason",
                table: "UserAccounts");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAccounts_Role_AccountType",
                table: "UserAccounts",
                sql: "(\r\n                        [Role] = 'Customer'\r\n                        AND [AccountType] IS NOT NULL\r\n                        AND [AccountType] IN\r\n                            ('LocalCustomer', 'ForeignTourist')\r\n                    )\r\n                    OR\r\n                    (\r\n                        [Role] IN ('Admin', 'KitchenStaff', 'Shipper')\r\n                        AND [AccountType] IS NULL\r\n                        AND [Username] IS NOT NULL\r\n                    )");
        }
    }
}
