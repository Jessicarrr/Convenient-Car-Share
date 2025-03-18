using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvenientCarShare.Migrations
{
    public partial class AddBroadcastAttributeToMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBroadcastMessage",
                table: "Messages",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBroadcastMessage",
                table: "Messages");
        }
    }
}
