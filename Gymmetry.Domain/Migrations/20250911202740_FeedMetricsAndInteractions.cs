using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class FeedMetricsAndInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchMedia_Branch_BranchId",
                table: "BranchMedia");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchService_BranchServiceType_BranchServiceTypeId",
                table: "BranchService");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchService_Branch_BranchId",
                table: "BranchService");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentOccupancy_Branch_BranchId",
                table: "CurrentOccupancy");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercise_DailyExerciseHistory_DailyExerciseHistoryId",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_DailyExerciseHistoryId",
                table: "Exercise");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GymImage",
                table: "GymImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CurrentOccupancy",
                table: "CurrentOccupancy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BranchServiceType",
                table: "BranchServiceType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BranchService",
                table: "BranchService");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BranchMedia",
                table: "BranchMedia");

            migrationBuilder.DropColumn(
                name: "DailyExerciseHistoryId",
                table: "Exercise");

            migrationBuilder.RenameTable(
                name: "GymImage",
                newName: "GymImages");

            migrationBuilder.RenameTable(
                name: "CurrentOccupancy",
                newName: "CurrentOccupancies");

            migrationBuilder.RenameTable(
                name: "BranchServiceType",
                newName: "BranchServiceTypes");

            migrationBuilder.RenameTable(
                name: "BranchService",
                newName: "BranchServices");

            migrationBuilder.RenameTable(
                name: "BranchMedia",
                newName: "BranchMedias");

            migrationBuilder.RenameIndex(
                name: "IX_CurrentOccupancy_BranchId",
                table: "CurrentOccupancies",
                newName: "IX_CurrentOccupancies_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_BranchService_BranchServiceTypeId",
                table: "BranchServices",
                newName: "IX_BranchServices_BranchServiceTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_BranchService_BranchId",
                table: "BranchServices",
                newName: "IX_BranchServices_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_BranchMedia_BranchId",
                table: "BranchMedias",
                newName: "IX_BranchMedias_BranchId");

            migrationBuilder.AddColumn<int>(
                name: "CommentsCount",
                table: "Feed",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikesCount",
                table: "Feed",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Percentage",
                table: "DailyHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "DailyExerciseHistory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "GymImages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "GymImages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "GymImages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "GymImages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "GymImages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "GymImages",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CurrentOccupancies",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "CurrentOccupancies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "CurrentOccupancies",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CurrentOccupancies",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "BranchServiceTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BranchServiceTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "BranchServiceTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BranchServiceTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "BranchServiceTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchServiceTypes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "BranchServices",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "BranchServices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "BranchServices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "BranchServices",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchServices",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "BranchMedias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "BranchMedias",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediaType",
                table: "BranchMedias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "BranchMedias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BranchMedias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "BranchMedias",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchMedias",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GymImages",
                table: "GymImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CurrentOccupancies",
                table: "CurrentOccupancies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BranchServiceTypes",
                table: "BranchServiceTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BranchServices",
                table: "BranchServices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BranchMedias",
                table: "BranchMedias",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FeedComment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedComment_Feed_FeedId",
                        column: x => x.FeedId,
                        principalTable: "Feed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedComment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedLike",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedLike_Feed_FeedId",
                        column: x => x.FeedId,
                        principalTable: "Feed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedLike_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedComment_FeedId",
                table: "FeedComment",
                column: "FeedId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedComment_UserId",
                table: "FeedComment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedLike_Feed_User",
                table: "FeedLike",
                columns: new[] { "FeedId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedLike_UserId",
                table: "FeedLike",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMedias_Branch_BranchId",
                table: "BranchMedias",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchServices_BranchServiceTypes_BranchServiceTypeId",
                table: "BranchServices",
                column: "BranchServiceTypeId",
                principalTable: "BranchServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchServices_Branch_BranchId",
                table: "BranchServices",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentOccupancies_Branch_BranchId",
                table: "CurrentOccupancies",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchMedias_Branch_BranchId",
                table: "BranchMedias");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchServices_BranchServiceTypes_BranchServiceTypeId",
                table: "BranchServices");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchServices_Branch_BranchId",
                table: "BranchServices");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentOccupancies_Branch_BranchId",
                table: "CurrentOccupancies");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory");

            migrationBuilder.DropTable(
                name: "FeedComment");

            migrationBuilder.DropTable(
                name: "FeedLike");

            migrationBuilder.DropIndex(
                name: "IX_FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GymImages",
                table: "GymImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CurrentOccupancies",
                table: "CurrentOccupancies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BranchServiceTypes",
                table: "BranchServiceTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BranchServices",
                table: "BranchServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BranchMedias",
                table: "BranchMedias");

            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Feed");

            migrationBuilder.DropColumn(
                name: "LikesCount",
                table: "Feed");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "DailyHistory");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "DailyExerciseHistory");

            migrationBuilder.RenameTable(
                name: "GymImages",
                newName: "GymImage");

            migrationBuilder.RenameTable(
                name: "CurrentOccupancies",
                newName: "CurrentOccupancy");

            migrationBuilder.RenameTable(
                name: "BranchServiceTypes",
                newName: "BranchServiceType");

            migrationBuilder.RenameTable(
                name: "BranchServices",
                newName: "BranchService");

            migrationBuilder.RenameTable(
                name: "BranchMedias",
                newName: "BranchMedia");

            migrationBuilder.RenameIndex(
                name: "IX_CurrentOccupancies_BranchId",
                table: "CurrentOccupancy",
                newName: "IX_CurrentOccupancy_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_BranchServices_BranchServiceTypeId",
                table: "BranchService",
                newName: "IX_BranchService_BranchServiceTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_BranchServices_BranchId",
                table: "BranchService",
                newName: "IX_BranchService_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_BranchMedias_BranchId",
                table: "BranchMedia",
                newName: "IX_BranchMedia_BranchId");

            migrationBuilder.AddColumn<Guid>(
                name: "DailyExerciseHistoryId",
                table: "Exercise",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "GymImage",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "GymImage",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "GymImage",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "GymImage",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "GymImage",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "GymImage",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CurrentOccupancy",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "CurrentOccupancy",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "CurrentOccupancy",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CurrentOccupancy",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "BranchServiceType",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BranchServiceType",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "BranchServiceType",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BranchServiceType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "BranchServiceType",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchServiceType",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "BranchService",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "BranchService",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "BranchService",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "BranchService",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchService",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "BranchMedia",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "BranchMedia",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediaType",
                table: "BranchMedia",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "BranchMedia",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BranchMedia",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "BranchMedia",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchMedia",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GymImage",
                table: "GymImage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CurrentOccupancy",
                table: "CurrentOccupancy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BranchServiceType",
                table: "BranchServiceType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BranchService",
                table: "BranchService",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BranchMedia",
                table: "BranchMedia",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FeedComment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedComment_Feed_FeedId",
                        column: x => x.FeedId,
                        principalTable: "Feed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedComment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedLike",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedLike_Feed_FeedId",
                        column: x => x.FeedId,
                        principalTable: "Feed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedLike_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedComment_FeedId",
                table: "FeedComment",
                column: "FeedId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedComment_UserId",
                table: "FeedComment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedLike_Feed_User",
                table: "FeedLike",
                columns: new[] { "FeedId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedLike_UserId",
                table: "FeedLike",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMedias_Branch_BranchId",
                table: "BranchMedias",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchServices_BranchServiceTypes_BranchServiceTypeId",
                table: "BranchServices",
                column: "BranchServiceTypeId",
                principalTable: "BranchServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchServices_Branch_BranchId",
                table: "BranchServices",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentOccupancies_Branch_BranchId",
                table: "CurrentOccupancies",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id");
        }
    }
}
