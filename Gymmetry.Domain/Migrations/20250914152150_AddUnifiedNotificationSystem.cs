using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedNotificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Notification",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotificationType",
                table: "Notification",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Notification",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateKey",
                table: "Notification",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WhatsAppTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequiresEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresSms = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowedChannels = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "push,app"),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationPreference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WhatsAppEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AppEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreference_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_IsActive",
                table: "NotificationTemplate",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Priority",
                table: "NotificationTemplate",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Type",
                table: "NotificationTemplate",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "UX_NotificationTemplate_TemplateKey",
                table: "NotificationTemplate",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreference_Type",
                table: "UserNotificationPreference",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreference_User",
                table: "UserNotificationPreference",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_UserNotificationPreference_UserType",
                table: "UserNotificationPreference",
                columns: new[] { "UserId", "NotificationType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTemplate");

            migrationBuilder.DropTable(
                name: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "NotificationType",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "TemplateKey",
                table: "Notification");
        }
    }
}
