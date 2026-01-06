using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class NomeDaMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ObservacaoCarga",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacaoDescarga",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TempoEsperaCarga",
                table: "Pedido",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TempoEsperaDescarga",
                table: "Pedido",
                type: "time",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObservacaoCarga",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "ObservacaoDescarga",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TempoEsperaCarga",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TempoEsperaDescarga",
                table: "Pedido");
        }
    }
}
