using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutineTemplateFKToRoutineAssigned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoutineTemplate_User_AuthorUserId",
                table: "RoutineTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_RoutineUser",
                table: "RoutineTemplate");

            migrationBuilder.DropIndex(
                name: "IX_FK_RoutineUser",
                table: "RoutineTemplate");

            migrationBuilder.DropIndex(
                name: "IX_RoutineTemplate_AuthorUserId",
                table: "RoutineTemplate");

            migrationBuilder.DropColumn(
                name: "AuthorUserId",
                table: "RoutineTemplate");

            migrationBuilder.DropColumn(
                name: "RoutineUser_RoutineId",
                table: "RoutineTemplate");

            migrationBuilder.RenameIndex(
                name: "IX_FK_UserDailyHistory",
                table: "DailyHistory",
                newName: "IX_DailyHistory_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "RoutineTemplateId",
                table: "RoutineAssigned",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FK_RoutineTemplateRoutineAssigned",
                table: "RoutineAssigned",
                column: "RoutineTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoutineTemplateRoutineAssigned",
                table: "RoutineAssigned",
                column: "RoutineTemplateId",
                principalTable: "RoutineTemplate",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoutineTemplateRoutineAssigned",
                table: "RoutineAssigned");

            migrationBuilder.DropIndex(
                name: "IX_FK_RoutineTemplateRoutineAssigned",
                table: "RoutineAssigned");

            migrationBuilder.DropColumn(
                name: "RoutineTemplateId",
                table: "RoutineAssigned");

            migrationBuilder.RenameIndex(
                name: "IX_DailyHistory_UserId",
                table: "DailyHistory",
                newName: "IX_FK_UserDailyHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserId",
                table: "RoutineTemplate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoutineUser_RoutineId",
                table: "RoutineTemplate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FK_RoutineUser",
                table: "RoutineTemplate",
                column: "RoutineUser_RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineTemplate_AuthorUserId",
                table: "RoutineTemplate",
                column: "AuthorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoutineTemplate_User_AuthorUserId",
                table: "RoutineTemplate",
                column: "AuthorUserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoutineUser",
                table: "RoutineTemplate",
                column: "RoutineUser_RoutineId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
