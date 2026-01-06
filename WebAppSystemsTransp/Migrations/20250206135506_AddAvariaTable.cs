using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AddAvariaTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocations_Pedido_PedidoId",
                table: "VehicleLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleLocations",
                table: "VehicleLocations");

            migrationBuilder.RenameTable(
                name: "VehicleLocations",
                newName: "VehicleLocation");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLocations_PedidoId",
                table: "VehicleLocation",
                newName: "IX_VehicleLocation_PedidoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleLocation",
                table: "VehicleLocation",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Avaria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    TipoAvaria = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaria_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaria_PedidoId",
                table: "Avaria",
                column: "PedidoId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocation_Pedido_PedidoId",
                table: "VehicleLocation",
                column: "PedidoId",
                principalTable: "Pedido",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocation_Pedido_PedidoId",
                table: "VehicleLocation");

            migrationBuilder.DropTable(
                name: "Avaria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleLocation",
                table: "VehicleLocation");

            migrationBuilder.RenameTable(
                name: "VehicleLocation",
                newName: "VehicleLocations");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLocation_PedidoId",
                table: "VehicleLocations",
                newName: "IX_VehicleLocations_PedidoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleLocations",
                table: "VehicleLocations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocations_Pedido_PedidoId",
                table: "VehicleLocations",
                column: "PedidoId",
                principalTable: "Pedido",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
