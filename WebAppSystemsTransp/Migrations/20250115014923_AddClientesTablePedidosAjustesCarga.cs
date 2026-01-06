using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AddClientesTablePedidosAjustesCarga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MotoristaId",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_MotoristaId",
                table: "Pedido",
                column: "MotoristaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Attorney_MotoristaId",
                table: "Pedido",
                column: "MotoristaId",
                principalTable: "Attorney",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Attorney_MotoristaId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_MotoristaId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "MotoristaId",
                table: "Pedido");
        }
    }
}
