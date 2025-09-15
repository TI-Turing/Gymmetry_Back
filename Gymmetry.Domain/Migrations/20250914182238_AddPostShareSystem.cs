using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPostShareSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SharedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SharedWith = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShareType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostShares", x => x.Id);
                    table.CheckConstraint("CK_PostShares_Platform", "Platform IN ('App', 'WhatsApp', 'Instagram', 'Facebook', 'Twitter', 'SMS', 'Email', 'Other')");
                    table.CheckConstraint("CK_PostShares_ShareType", "ShareType IN ('Internal', 'External')");
                    table.ForeignKey(
                        name: "FK_PostShares_Posts",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostShares_SharedBy",
                        column: x => x.SharedBy,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostShares_SharedWith",
                        column: x => x.SharedWith,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostShares_CreatedAt",
                table: "PostShares",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostShares_Post_Platform",
                table: "PostShares",
                columns: new[] { "PostId", "Platform" });

            migrationBuilder.CreateIndex(
                name: "IX_PostShares_PostId",
                table: "PostShares",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostShares_SharedBy",
                table: "PostShares",
                column: "SharedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostShares_SharedBy_CreatedAt",
                table: "PostShares",
                columns: new[] { "SharedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PostShares_SharedWith",
                table: "PostShares",
                column: "SharedWith");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostShares");
        }
    }
}
