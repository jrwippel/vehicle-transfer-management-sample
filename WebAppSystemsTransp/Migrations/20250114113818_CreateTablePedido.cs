using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class CreateTablePedido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataPedido = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraPedido = table.Column<TimeSpan>(type: "time", nullable: false),
                    DataPretendida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocalCarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdicionalCarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalDescarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdicionalDescarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoPedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pedido");
        }
    }
}
