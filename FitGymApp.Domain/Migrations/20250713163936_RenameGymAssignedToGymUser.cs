using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitGymApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RenameGymAssignedToGymUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GymUser",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGym",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "UserGym_UserId",
                table: "User",
                newName: "GymUser_Id");

            migrationBuilder.RenameIndex(
                name: "IX_FK_UserGym",
                table: "User",
                newName: "IX_FK_UserGymUser");

            migrationBuilder.RenameIndex(
                name: "IX_FK_GymUser",
                table: "User",
                newName: "IX_User_GymId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGymUser",
                table: "User",
                column: "GymUser_Id",
                principalTable: "Gym",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Gym_GymId",
                table: "User",
                column: "GymId",
                principalTable: "Gym",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGymUser",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Gym_GymId",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "GymUser_Id",
                table: "User",
                newName: "UserGym_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_User_GymId",
                table: "User",
                newName: "IX_FK_GymUser");

            migrationBuilder.RenameIndex(
                name: "IX_FK_UserGymUser",
                table: "User",
                newName: "IX_FK_UserGym");

            migrationBuilder.AddForeignKey(
                name: "FK_GymUser",
                table: "User",
                column: "GymId",
                principalTable: "Gym",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGym",
                table: "User",
                column: "UserGym_UserId",
                principalTable: "Gym",
                principalColumn: "Id");
        }
    }
}
