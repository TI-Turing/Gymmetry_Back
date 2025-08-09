using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsMuscleToExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TagsMuscle",
                table: "Exercise",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagsMuscle",
                table: "Exercise");
        }
    }
}
