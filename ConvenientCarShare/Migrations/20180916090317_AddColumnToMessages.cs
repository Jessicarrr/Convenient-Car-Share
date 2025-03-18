using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvenientCarShare.Migrations
{
    public partial class AddColumnToMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBeenRead",
                table: "Messages",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBeenRead",
                table: "Messages");
        }
    }
}
