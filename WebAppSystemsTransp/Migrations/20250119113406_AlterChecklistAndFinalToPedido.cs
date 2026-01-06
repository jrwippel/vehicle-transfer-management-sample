using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AlterChecklistAndFinalToPedido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColeteHomologado",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocumentoSeguro",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocumentoVeiculo",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrianguloHomologado",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColeteHomologado",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "DocumentoSeguro",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "DocumentoVeiculo",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TrianguloHomologado",
                table: "Pedido");
        }
    }
}
