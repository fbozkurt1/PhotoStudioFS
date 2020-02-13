using Microsoft.EntityFrameworkCore.Migrations;

namespace PhotoStudioFS.Migrations
{
    public partial class AddDescriptionToShootType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ShootTypes",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ShootTypes");
        }
    }
}
