using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBlockSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBlock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlock", x => x.Id);
                    table.CheckConstraint("CK_UserBlock_NoSelfBlock", "BlockerId != BlockedUserId");
                    table.ForeignKey(
                        name: "FK_UserBlock_BlockedUser",
                        column: x => x.BlockedUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBlock_Blocker",
                        column: x => x.BlockerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBlock_Blocked",
                table: "UserBlock",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlock_Blocker",
                table: "UserBlock",
                column: "BlockerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlock_CreatedAt",
                table: "UserBlock",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlock_IsActive",
                table: "UserBlock",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "UX_UserBlock_BlockerBlocked",
                table: "UserBlock",
                columns: new[] { "BlockerId", "BlockedUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBlock");
        }
    }
}
