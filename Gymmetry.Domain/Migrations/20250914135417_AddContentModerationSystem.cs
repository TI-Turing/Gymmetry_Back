using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddContentModerationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentModeration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<int>(type: "int", nullable: false),
                    ModerationAction = table.Column<int>(type: "int", nullable: false),
                    ModerationReason = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    ModeratedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModeratedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    AutoModerated = table.Column<bool>(type: "bit", nullable: false),
                    ReviewRequired = table.Column<bool>(type: "bit", nullable: false),
                    FilterType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Confidence = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentModeration", x => x.Id);
                    table.CheckConstraint("CK_ContentModeration_Confidence", "Confidence IS NULL OR (Confidence >= 0 AND Confidence <= 1)");
                    table.ForeignKey(
                        name: "FK_ContentModeration_Moderator",
                        column: x => x.ModeratedBy,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_Action",
                table: "ContentModeration",
                column: "ModerationAction");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_AutoModerated",
                table: "ContentModeration",
                column: "AutoModerated");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_Content",
                table: "ContentModeration",
                columns: new[] { "ContentId", "ContentType" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_ContentId",
                table: "ContentModeration",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_ContentType",
                table: "ContentModeration",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_IsActive",
                table: "ContentModeration",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_ModeratedAt",
                table: "ContentModeration",
                column: "ModeratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_ModeratedBy",
                table: "ContentModeration",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModeration_ReviewRequired",
                table: "ContentModeration",
                column: "ReviewRequired");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentModeration");
        }
    }
}
