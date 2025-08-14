using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBranchesCollectionFromDaily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branch_Daily_DailyId",
                table: "Branch");

            migrationBuilder.DropForeignKey(
                name: "FK_Daily_RoutineExercise_RoutineExerciseId",
                table: "Daily");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyExercise_Daily_DailyId",
                table: "DailyExercise");

            migrationBuilder.DropIndex(
                name: "IX_DailyExercise_DailyId",
                table: "DailyExercise");

            migrationBuilder.DropIndex(
                name: "IX_Daily_RoutineExerciseId",
                table: "Daily");

            migrationBuilder.DropIndex(
                name: "IX_Branch_DailyId",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "DailyId",
                table: "DailyExercise");

            migrationBuilder.DropColumn(
                name: "RoutineExerciseId",
                table: "Daily");

            migrationBuilder.DropColumn(
                name: "DailyId",
                table: "Branch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DailyId",
                table: "DailyExercise",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoutineExerciseId",
                table: "Daily",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DailyId",
                table: "Branch",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyExercise_DailyId",
                table: "DailyExercise",
                column: "DailyId");

            migrationBuilder.CreateIndex(
                name: "IX_Daily_RoutineExerciseId",
                table: "Daily",
                column: "RoutineExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_DailyId",
                table: "Branch",
                column: "DailyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branch_Daily_DailyId",
                table: "Branch",
                column: "DailyId",
                principalTable: "Daily",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Daily_RoutineExercise_RoutineExerciseId",
                table: "Daily",
                column: "RoutineExerciseId",
                principalTable: "RoutineExercise",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyExercise_Daily_DailyId",
                table: "DailyExercise",
                column: "DailyId",
                principalTable: "Daily",
                principalColumn: "Id");
        }
    }
}
