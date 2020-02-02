using Microsoft.EntityFrameworkCore.Migrations;

namespace PhotoStudioFS.Migrations
{
    public partial class newMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShootTypes_ShootTypes_ShootTypeId",
                table: "ShootTypes");

            migrationBuilder.DropIndex(
                name: "IX_ShootTypes_ShootTypeId",
                table: "ShootTypes");

            migrationBuilder.DropColumn(
                name: "ShootTypeId",
                table: "ShootTypes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShootTypeId",
                table: "ShootTypes",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShootTypes_ShootTypeId",
                table: "ShootTypes",
                column: "ShootTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShootTypes_ShootTypes_ShootTypeId",
                table: "ShootTypes",
                column: "ShootTypeId",
                principalTable: "ShootTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
