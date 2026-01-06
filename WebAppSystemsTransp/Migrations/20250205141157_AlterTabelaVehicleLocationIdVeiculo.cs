using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AlterTabelaVehicleLocationIdVeiculo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "VehicleLocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLocations_VehicleId",
                table: "VehicleLocations",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocations_Veiculo_VehicleId",
                table: "VehicleLocations",
                column: "VehicleId",
                principalTable: "Veiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocations_Veiculo_VehicleId",
                table: "VehicleLocations");

            migrationBuilder.DropIndex(
                name: "IX_VehicleLocations_VehicleId",
                table: "VehicleLocations");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "VehicleLocations");
        }
    }
}
