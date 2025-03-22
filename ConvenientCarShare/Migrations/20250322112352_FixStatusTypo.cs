using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenientCarShare.Migrations
{
    /// <inheritdoc />
    public partial class FixStatusTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "Bookings",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Bookings",
                newName: "status");
        }
    }
}
