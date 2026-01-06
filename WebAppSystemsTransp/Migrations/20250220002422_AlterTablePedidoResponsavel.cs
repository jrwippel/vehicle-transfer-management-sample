using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AlterTablePedidoResponsavel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailPessoaCar",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailPessoaDes",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomePessoaCar",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomePessoaDes",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelPessoaCar",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelPessoaDes",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailPessoaCar",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EmailPessoaDes",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "NomePessoaCar",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "NomePessoaDes",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TelPessoaCar",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TelPessoaDes",
                table: "Pedido");
        }
    }
}
