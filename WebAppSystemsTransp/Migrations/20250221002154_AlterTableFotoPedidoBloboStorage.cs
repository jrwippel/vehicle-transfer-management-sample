using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AlterTableFotoPedidoBloboStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "FotoPedido");

            migrationBuilder.RenameColumn(
                name: "TipoArquivo",
                table: "FotoPedido",
                newName: "UrlFoto");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UrlFoto",
                table: "FotoPedido",
                newName: "TipoArquivo");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "FotoPedido",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
