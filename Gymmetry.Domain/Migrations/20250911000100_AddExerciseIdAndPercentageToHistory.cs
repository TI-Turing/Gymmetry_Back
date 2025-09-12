using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    public partial class AddExerciseIdAndPercentageToHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DailyHistory.Percentage
            migrationBuilder.AddColumn<int>(
                name: "Percentage",
                table: "DailyHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // DailyExerciseHistory.ExerciseId
            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "DailyExerciseHistory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory",
                column: "ExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory");

            migrationBuilder.DropIndex(
                name: "IX_FK_ExerciseDailyExerciseHistory_Historic",
                table: "DailyExerciseHistory");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "DailyExerciseHistory");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "DailyHistory");
        }
    }
}
