using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    public partial class AddPaymentIntentExpiration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "PaymentIntents",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusCheckAt",
                table: "PaymentIntents",
                type: "datetime2",
                nullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_PaymentIntents_Status_ExpiresAt",
                table: "PaymentIntents",
                columns: new[] { "Status", "ExpiresAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentIntents_Status_ExpiresAt",
                table: "PaymentIntents");
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "PaymentIntents");
            migrationBuilder.DropColumn(
                name: "LastStatusCheckAt",
                table: "PaymentIntents");
        }
    }
}
