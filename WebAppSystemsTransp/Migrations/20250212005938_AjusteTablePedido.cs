using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AjusteTablePedido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleLocation");

            migrationBuilder.DropColumn(
                name: "AssClienteEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "AssClienteRecolha",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "AssMotoristaEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "AssMotoristaRecolha",
                table: "Pedido");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssClienteEntrega",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssClienteRecolha",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssMotoristaEntrega",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssMotoristaRecolha",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VehicleLocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleLocation_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLocation_PedidoId",
                table: "VehicleLocation",
                column: "PedidoId");
        }
    }
}
