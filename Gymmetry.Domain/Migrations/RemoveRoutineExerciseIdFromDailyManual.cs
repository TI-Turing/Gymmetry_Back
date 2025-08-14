using Microsoft.EntityFrameworkCore.Migrations;

namespace Gymmetry.Domain.Migrations
{
    public partial class RemoveRoutineExerciseIdFromDailyManual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoutineExerciseId",
                table: "Daily");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoutineExerciseId",
                table: "Daily",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);
        }
    }
}
