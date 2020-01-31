using Microsoft.EntityFrameworkCore.Migrations;

namespace PhotoStudioFS.Migrations
{
    public partial class addShootTypeFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "photoShootType",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "ShootTypeId",
                table: "Schedules",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShootTypeId",
                table: "Appointments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ShootTypeId",
                table: "Schedules",
                column: "ShootTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ShootTypeId",
                table: "Appointments",
                column: "ShootTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ShootTypes_ShootTypeId",
                table: "Appointments",
                column: "ShootTypeId",
                principalTable: "ShootTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_ShootTypes_ShootTypeId",
                table: "Schedules",
                column: "ShootTypeId",
                principalTable: "ShootTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ShootTypes_ShootTypeId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_ShootTypes_ShootTypeId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ShootTypeId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ShootTypeId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ShootTypeId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ShootTypeId",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "photoShootType",
                table: "Schedules",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Appointments",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
