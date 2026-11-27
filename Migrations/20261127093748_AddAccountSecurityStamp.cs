using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonBite.Migrations
{
    public partial class AddAccountSecurityStamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add nullable SecurityStamp column
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "UserAccounts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            // 2. Generate a unique stamp for existing accounts
            migrationBuilder.Sql(@"
                UPDATE [UserAccounts]
                SET [SecurityStamp] =
                    LOWER(
                        REPLACE(
                            CONVERT(NVARCHAR(36), NEWID()),
                            '-', ''
                        )
                    )
                WHERE [SecurityStamp] IS NULL
                   OR [SecurityStamp] = '';
            ");

            // 3. Make SecurityStamp required
            migrationBuilder.AlterColumn<string>(
                name: "SecurityStamp",
                table: "UserAccounts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "UserAccounts");
        }
    }
}