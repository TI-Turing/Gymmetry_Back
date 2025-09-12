using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymmetry.Domain.Migrations
{
    public partial class SyncFullModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty sync migration: no runtime changes, snapshot already matches DB.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to rollback.
        }
    }
}
