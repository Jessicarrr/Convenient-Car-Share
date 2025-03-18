using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvenientCarShare.Migrations
{
    public partial class AddCarTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Brand = table.Column<string>(nullable: false),
                    Model = table.Column<string>(nullable: false),
                    Colour = table.Column<string>(nullable: false),
                    NumberPlate = table.Column<string>(nullable: false),
                    CurrentlyParkedAtId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cars_ParkingAreas_CurrentlyParkedAtId",
                        column: x => x.CurrentlyParkedAtId,
                        principalTable: "ParkingAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_CurrentlyParkedAtId",
                table: "Cars",
                column: "CurrentlyParkedAtId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
