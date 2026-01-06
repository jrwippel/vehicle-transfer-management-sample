using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AlterTableVehicleLocationPedido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocations_Veiculo_VehicleId",
                table: "VehicleLocations");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "VehicleLocations",
                newName: "PedidoId");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLocations_VehicleId",
                table: "VehicleLocations",
                newName: "IX_VehicleLocations_PedidoId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocations_Pedido_PedidoId",
                table: "VehicleLocations",
                column: "PedidoId",
                principalTable: "Pedido",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocations_Pedido_PedidoId",
                table: "VehicleLocations");

            migrationBuilder.RenameColumn(
                name: "PedidoId",
                table: "VehicleLocations",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLocations_PedidoId",
                table: "VehicleLocations",
                newName: "IX_VehicleLocations_VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocations_Veiculo_VehicleId",
                table: "VehicleLocations",
                column: "VehicleId",
                principalTable: "Veiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
