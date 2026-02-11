using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivoCancelacionToPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesDePedidos_Pedidos_PedidoIDPedido",
                table: "DetallesDePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesDePedidos_Productos_ProductoIDProducto",
                table: "DetallesDePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialesDeEstados_EstadosDePedidos_EstadoDePedidoIDEstadoDePedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialesDeEstados_Pedidos_PedidoIDPedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialesDeEstados_Usuarios_UsuarioIDUsuario",
                table: "HistorialesDeEstados");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Sucursales_SucursalIDSucursal",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_SucursalIDSucursal",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_HistorialesDeEstados_EstadoDePedidoIDEstadoDePedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropIndex(
                name: "IX_HistorialesDeEstados_PedidoIDPedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropIndex(
                name: "IX_HistorialesDeEstados_UsuarioIDUsuario",
                table: "HistorialesDeEstados");

            migrationBuilder.DropIndex(
                name: "IX_DetallesDePedidos_PedidoIDPedido",
                table: "DetallesDePedidos");

            migrationBuilder.DropIndex(
                name: "IX_DetallesDePedidos_ProductoIDProducto",
                table: "DetallesDePedidos");

            migrationBuilder.DropColumn(
                name: "SucursalIDSucursal",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EstadoDePedidoIDEstadoDePedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropColumn(
                name: "PedidoIDPedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropColumn(
                name: "UsuarioIDUsuario",
                table: "HistorialesDeEstados");

            migrationBuilder.DropColumn(
                name: "PedidoIDPedido",
                table: "DetallesDePedidos");

            migrationBuilder.DropColumn(
                name: "ProductoIDProducto",
                table: "DetallesDePedidos");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JustificacionCancelacion",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MotivoCancelacionId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MotivosCancelacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosCancelacion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IDSucursal",
                table: "Usuarios",
                column: "IDSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_MotivoCancelacionId",
                table: "Pedidos",
                column: "MotivoCancelacionId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesDeEstados_IDEstadoDePedido",
                table: "HistorialesDeEstados",
                column: "IDEstadoDePedido");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesDeEstados_IDPedido",
                table: "HistorialesDeEstados",
                column: "IDPedido");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesDeEstados_IDUsuario",
                table: "HistorialesDeEstados",
                column: "IDUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDePedidos_IDPedido",
                table: "DetallesDePedidos",
                column: "IDPedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDePedidos_IDProducto",
                table: "DetallesDePedidos",
                column: "IDProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesDePedidos_Pedidos_IDPedido",
                table: "DetallesDePedidos",
                column: "IDPedido",
                principalTable: "Pedidos",
                principalColumn: "IDPedido",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesDePedidos_Productos_IDProducto",
                table: "DetallesDePedidos",
                column: "IDProducto",
                principalTable: "Productos",
                principalColumn: "IDProducto",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialesDeEstados_EstadosDePedidos_IDEstadoDePedido",
                table: "HistorialesDeEstados",
                column: "IDEstadoDePedido",
                principalTable: "EstadosDePedidos",
                principalColumn: "IDEstadoDePedido",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialesDeEstados_Pedidos_IDPedido",
                table: "HistorialesDeEstados",
                column: "IDPedido",
                principalTable: "Pedidos",
                principalColumn: "IDPedido",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialesDeEstados_Usuarios_IDUsuario",
                table: "HistorialesDeEstados",
                column: "IDUsuario",
                principalTable: "Usuarios",
                principalColumn: "IDUsuario",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_MotivosCancelacion_MotivoCancelacionId",
                table: "Pedidos",
                column: "MotivoCancelacionId",
                principalTable: "MotivosCancelacion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Sucursales_IDSucursal",
                table: "Usuarios",
                column: "IDSucursal",
                principalTable: "Sucursales",
                principalColumn: "IDSucursal",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesDePedidos_Pedidos_IDPedido",
                table: "DetallesDePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesDePedidos_Productos_IDProducto",
                table: "DetallesDePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialesDeEstados_EstadosDePedidos_IDEstadoDePedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialesDeEstados_Pedidos_IDPedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialesDeEstados_Usuarios_IDUsuario",
                table: "HistorialesDeEstados");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_MotivosCancelacion_MotivoCancelacionId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Sucursales_IDSucursal",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "MotivosCancelacion");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_IDSucursal",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_MotivoCancelacionId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_HistorialesDeEstados_IDEstadoDePedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropIndex(
                name: "IX_HistorialesDeEstados_IDPedido",
                table: "HistorialesDeEstados");

            migrationBuilder.DropIndex(
                name: "IX_HistorialesDeEstados_IDUsuario",
                table: "HistorialesDeEstados");

            migrationBuilder.DropIndex(
                name: "IX_DetallesDePedidos_IDPedido",
                table: "DetallesDePedidos");

            migrationBuilder.DropIndex(
                name: "IX_DetallesDePedidos_IDProducto",
                table: "DetallesDePedidos");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "JustificacionCancelacion",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "MotivoCancelacionId",
                table: "Pedidos");

            migrationBuilder.AddColumn<int>(
                name: "SucursalIDSucursal",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstadoDePedidoIDEstadoDePedido",
                table: "HistorialesDeEstados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PedidoIDPedido",
                table: "HistorialesDeEstados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioIDUsuario",
                table: "HistorialesDeEstados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PedidoIDPedido",
                table: "DetallesDePedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductoIDProducto",
                table: "DetallesDePedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_SucursalIDSucursal",
                table: "Usuarios",
                column: "SucursalIDSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesDeEstados_EstadoDePedidoIDEstadoDePedido",
                table: "HistorialesDeEstados",
                column: "EstadoDePedidoIDEstadoDePedido");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesDeEstados_PedidoIDPedido",
                table: "HistorialesDeEstados",
                column: "PedidoIDPedido");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesDeEstados_UsuarioIDUsuario",
                table: "HistorialesDeEstados",
                column: "UsuarioIDUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDePedidos_PedidoIDPedido",
                table: "DetallesDePedidos",
                column: "PedidoIDPedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDePedidos_ProductoIDProducto",
                table: "DetallesDePedidos",
                column: "ProductoIDProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesDePedidos_Pedidos_PedidoIDPedido",
                table: "DetallesDePedidos",
                column: "PedidoIDPedido",
                principalTable: "Pedidos",
                principalColumn: "IDPedido",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesDePedidos_Productos_ProductoIDProducto",
                table: "DetallesDePedidos",
                column: "ProductoIDProducto",
                principalTable: "Productos",
                principalColumn: "IDProducto",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialesDeEstados_EstadosDePedidos_EstadoDePedidoIDEstadoDePedido",
                table: "HistorialesDeEstados",
                column: "EstadoDePedidoIDEstadoDePedido",
                principalTable: "EstadosDePedidos",
                principalColumn: "IDEstadoDePedido",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialesDeEstados_Pedidos_PedidoIDPedido",
                table: "HistorialesDeEstados",
                column: "PedidoIDPedido",
                principalTable: "Pedidos",
                principalColumn: "IDPedido",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialesDeEstados_Usuarios_UsuarioIDUsuario",
                table: "HistorialesDeEstados",
                column: "UsuarioIDUsuario",
                principalTable: "Usuarios",
                principalColumn: "IDUsuario",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Sucursales_SucursalIDSucursal",
                table: "Usuarios",
                column: "SucursalIDSucursal",
                principalTable: "Sucursales",
                principalColumn: "IDSucursal",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
