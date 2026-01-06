using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class MakeMotoristaIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Attorney_MotoristaId",
                table: "Pedido");

            migrationBuilder.AlterColumn<int>(
                name: "MotoristaId",
                table: "Pedido",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Attorney_MotoristaId",
                table: "Pedido",
                column: "MotoristaId",
                principalTable: "Attorney",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Attorney_MotoristaId",
                table: "Pedido");

            migrationBuilder.AlterColumn<int>(
                name: "MotoristaId",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Attorney_MotoristaId",
                table: "Pedido",
                column: "MotoristaId",
                principalTable: "Attorney",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
