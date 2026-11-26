using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonBite.Migrations
{
    public partial class LinkCustomerToUserAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add nullable UserId to Customers
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Customers",
                type: "int",
                nullable: true);

            // 2. Ensure one customer per user account
            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            // 3. Link Customers to UserAccounts
            migrationBuilder.AddForeignKey(
                name: "FK_Customers_UserAccounts_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_UserAccounts_UserId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customers");
        }
    }
}