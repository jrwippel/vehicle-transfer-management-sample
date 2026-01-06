using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AddVeiculoToPedido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VeiculoId",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_VeiculoId",
                table: "Pedido",
                column: "VeiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Veiculo_VeiculoId",
                table: "Pedido",
                column: "VeiculoId",
                principalTable: "Veiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Veiculo_VeiculoId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_VeiculoId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "VeiculoId",
                table: "Pedido");
        }
    }
}
