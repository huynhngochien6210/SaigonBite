using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonBite.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccountManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.UserId);
                    table.CheckConstraint("CK_UserAccounts_Role_AccountType", "(\r\n                        [Role] = 'Customer'\r\n                        AND [AccountType] IS NOT NULL\r\n                        AND [AccountType] IN\r\n                            ('LocalCustomer', 'ForeignTourist')\r\n                    )\r\n                    OR\r\n                    (\r\n                        [Role] IN ('Admin', 'KitchenStaff', 'Shipper')\r\n                        AND [AccountType] IS NULL\r\n                        AND [Username] IS NOT NULL\r\n                    )");
                });

            migrationBuilder.CreateTable(
                name: "ForeignTourists",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForeignTourists", x => x.UserId);
                    table.CheckConstraint("CK_ForeignTourists_PassportNumber", "LEN([PassportNumber]) BETWEEN 6 AND 9\r\n                      AND [PassportNumber] COLLATE Latin1_General_100_BIN2\r\n                          NOT LIKE '%[^A-Za-z0-9]%'");
                    table.ForeignKey(
                        name: "FK_ForeignTourists_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocalCustomers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalCustomers", x => x.UserId);
                    table.CheckConstraint("CK_LocalCustomers_PhoneNumber", "LEN([PhoneNumber]) = 10\r\n                      AND LEFT([PhoneNumber], 2)\r\n                          IN ('03', '05', '07', '08', '09')\r\n                      AND [PhoneNumber] COLLATE Latin1_General_100_BIN2\r\n                          NOT LIKE '%[^0-9]%'");
                    table.ForeignKey(
                        name: "FK_LocalCustomers_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResets",
                columns: table => new
                {
                    ResetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResets", x => x.ResetId);
                    table.CheckConstraint("CK_PasswordResets_FailedAttempts", "[FailedAttempts] >= 0");
                    table.ForeignKey(
                        name: "FK_PasswordResets_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    VerificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.VerificationId);
                    table.CheckConstraint("CK_VerificationCodes_FailedAttempts", "[FailedAttempts] >= 0");
                    table.ForeignKey(
                        name: "FK_VerificationCodes_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForeignTourists_Email",
                table: "ForeignTourists",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForeignTourists_PassportNumber",
                table: "ForeignTourists",
                column: "PassportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalCustomers_PhoneNumber",
                table: "LocalCustomers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_TokenHash",
                table: "PasswordResets",
                column: "TokenHash",
                unique: true,
                filter: "[TokenHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId_IsUsed_ExpiresAt",
                table: "PasswordResets",
                columns: new[] { "UserId", "IsUsed", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Username",
                table: "UserAccounts",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_UserId_IsUsed_ExpiresAt",
                table: "VerificationCodes",
                columns: new[] { "UserId", "IsUsed", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForeignTourists");

            migrationBuilder.DropTable(
                name: "LocalCustomers");

            migrationBuilder.DropTable(
                name: "PasswordResets");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
