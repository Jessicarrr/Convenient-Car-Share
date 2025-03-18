using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvenientCarShare.Migrations
{
    public partial class statusForBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Bookings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "Bookings");
        }
    }
}
