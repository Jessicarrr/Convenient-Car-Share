using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvenientCarShare.Migrations
{
    public partial class warnned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Warnned",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Warnned",
                table: "AspNetUsers");
        }
    }
}
