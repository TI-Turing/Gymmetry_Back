using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class DailyDailyExerciseRelationFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyDailyExercise",
                table: "DailyExercise");

            migrationBuilder.RenameIndex(
                name: "IX_FK_DailyDailyExercise",
                table: "DailyExercise",
                newName: "IX_DailyExercise_DailyId");

            migrationBuilder.AlterColumn<Guid>(
                name: "DailyId",
                table: "DailyExercise",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "DailyExerciseId",
                table: "Daily",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FK_DailyExerciseDaily",
                table: "Daily",
                column: "DailyExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyExerciseDaily",
                table: "Daily",
                column: "DailyExerciseId",
                principalTable: "DailyExercise",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyExercise_Daily_DailyId",
                table: "DailyExercise",
                column: "DailyId",
                principalTable: "Daily",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyExerciseDaily",
                table: "Daily");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyExercise_Daily_DailyId",
                table: "DailyExercise");

            migrationBuilder.DropIndex(
                name: "IX_FK_DailyExerciseDaily",
                table: "Daily");

            migrationBuilder.DropColumn(
                name: "DailyExerciseId",
                table: "Daily");

            migrationBuilder.RenameIndex(
                name: "IX_DailyExercise_DailyId",
                table: "DailyExercise",
                newName: "IX_FK_DailyDailyExercise");

            migrationBuilder.AlterColumn<Guid>(
                name: "DailyId",
                table: "DailyExercise",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyDailyExercise",
                table: "DailyExercise",
                column: "DailyId",
                principalTable: "Daily",
                principalColumn: "Id");
        }
    }
}
