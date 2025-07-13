using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitGymApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsdPriceLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GymPlanSelectedType",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "GymPlanSelectedType",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "GymPlanSelectedType",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "GymPlanSelectedType",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "GymPlanSelectedType",
                columns: new[] { "Id", "CountryId", "CreatedAt", "DeletedAt", "Description", "Ip", "IsActive", "Name", "Price", "UpdatedAt", "UsdPrice" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Plan Básico - $20.000 COP/mes.", null, true, "Plan Básico", 20000m, null, 5m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Plan Pro (Gestión + Rutinas) - $45.000 COP/mes o $2.000 COP por cliente activo.", null, true, "Plan Pro (Gestión + Rutinas)", 45000m, null, 11m },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Plan Premium - $70.000 COP/mes.", null, true, "Plan Premium", 70000m, null, 17m },
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Plan Corporativo / White Label - Precio null.", null, true, "Plan Corporativo / White Label", null, null, null }
                });
        }
    }
}
