using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Migrations
{
    /// <inheritdoc />
    public partial class AddZonasSystemToDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ZonaId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZonaId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZonaId",
                table: "Barrios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Zonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zonas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ZonaId",
                table: "Usuarios",
                column: "ZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ZonaId",
                table: "Pedidos",
                column: "ZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Barrios_ZonaId",
                table: "Barrios",
                column: "ZonaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Barrios_Zonas_ZonaId",
                table: "Barrios",
                column: "ZonaId",
                principalTable: "Zonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Zonas_ZonaId",
                table: "Pedidos",
                column: "ZonaId",
                principalTable: "Zonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Zonas_ZonaId",
                table: "Usuarios",
                column: "ZonaId",
                principalTable: "Zonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barrios_Zonas_ZonaId",
                table: "Barrios");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Zonas_ZonaId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Zonas_ZonaId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Zonas");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_ZonaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_ZonaId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Barrios_ZonaId",
                table: "Barrios");

            migrationBuilder.DropColumn(
                name: "ZonaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ZonaId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ZonaId",
                table: "Barrios");
        }
    }
}
