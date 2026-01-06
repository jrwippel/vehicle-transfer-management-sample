using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AlterTablePedidoAssinaturas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssinaturaInicial",
                table: "Pedido",
                newName: "AssMotoristaRecolha");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssClienteEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "AssClienteRecolha",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "AssMotoristaEntrega",
                table: "Pedido");

            migrationBuilder.RenameColumn(
                name: "AssMotoristaRecolha",
                table: "Pedido",
                newName: "AssinaturaInicial");
        }
    }
}
