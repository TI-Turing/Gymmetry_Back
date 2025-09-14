using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddReportContentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportedContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<int>(type: "int", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportContent_ReportedUser",
                        column: x => x.ReportedUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportContent_Reporter",
                        column: x => x.ReporterId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportContent_Reviewer",
                        column: x => x.ReviewedBy,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReportContentAudit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportContentAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportContentAudit_Report",
                        column: x => x.ReportContentId,
                        principalTable: "ReportContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportContentEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportContentEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportContentEvidence_Report",
                        column: x => x.ReportContentId,
                        principalTable: "ReportContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportContentEvidence_ReportContent_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportContent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportContent_Content",
                table: "ReportContent",
                columns: new[] { "ReportedContentId", "ContentType" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportContent_ReportedUserId",
                table: "ReportContent",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportContent_ReviewedBy",
                table: "ReportContent",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportContent_Status_Priority",
                table: "ReportContent",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "UX_ReportContent_UniqueReporterContent",
                table: "ReportContent",
                columns: new[] { "ReporterId", "ReportedContentId", "ContentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportContentAudit_Report",
                table: "ReportContentAudit",
                column: "ReportContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportContentEvidence_Report",
                table: "ReportContentEvidence",
                column: "ReportContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportContentEvidence_ReportId",
                table: "ReportContentEvidence",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportContentAudit");

            migrationBuilder.DropTable(
                name: "ReportContentEvidence");

            migrationBuilder.DropTable(
                name: "ReportContent");
        }
    }
}
