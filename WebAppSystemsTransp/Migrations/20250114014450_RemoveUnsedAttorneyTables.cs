using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class RemoveUnsedAttorneyTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Attorney");

            migrationBuilder.DropColumn(
                name: "UseBorder",
                table: "Attorney");

            migrationBuilder.DropColumn(
                name: "UseCronometroAlwaysOnTop",
                table: "Attorney");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Attorney",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UseBorder",
                table: "Attorney",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseCronometroAlwaysOnTop",
                table: "Attorney",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
