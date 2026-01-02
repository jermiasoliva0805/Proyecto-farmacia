using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrationV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadosDePedidos",
                columns: table => new
                {
                    IDEstadoDePedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    motivo_cancelacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosDePedidos", x => x.IDEstadoDePedido);
                });

            migrationBuilder.CreateTable(
                name: "Localidades",
                columns: table => new
                {
                    IDLocalidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barrio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoPostal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provincia = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localidades", x => x.IDLocalidad);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    IDProducto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreProducto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadProducto = table.Column<int>(type: "int", nullable: false),
                    PrecioProducto = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.IDProducto);
                });

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    IDSucursal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreSucursal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dirección = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Teléfono = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.IDSucursal);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    IDCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DNI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDLocalidad = table.Column<int>(type: "int", nullable: false),
                    LocalidadIDLocalidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IDCliente);
                    table.ForeignKey(
                        name: "FK_Clientes_Localidades_LocalidadIDLocalidad",
                        column: x => x.LocalidadIDLocalidad,
                        principalTable: "Localidades",
                        principalColumn: "IDLocalidad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IDUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDSucursal = table.Column<int>(type: "int", nullable: false),
                    SucursalIDSucursal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IDUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Sucursales_SucursalIDSucursal",
                        column: x => x.SucursalIDSucursal,
                        principalTable: "Sucursales",
                        principalColumn: "IDSucursal",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    IDPedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FormaDePago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoActual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEntregaReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEntregaEstimada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraEntregaReal = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraEntregaEstimada = table.Column<TimeSpan>(type: "time", nullable: false),
                    IDCliente = table.Column<int>(type: "int", nullable: false),
                    IDEstadoDePedido = table.Column<int>(type: "int", nullable: false),
                    IDUsuario = table.Column<int>(type: "int", nullable: false),
                    IDSucursal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.IDPedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente");
                    table.ForeignKey(
                        name: "FK_Pedidos_EstadosDePedidos_IDEstadoDePedido",
                        column: x => x.IDEstadoDePedido,
                        principalTable: "EstadosDePedidos",
                        principalColumn: "IDEstadoDePedido");
                    table.ForeignKey(
                        name: "FK_Pedidos_Sucursales_IDSucursal",
                        column: x => x.IDSucursal,
                        principalTable: "Sucursales",
                        principalColumn: "IDSucursal");
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_IDUsuario",
                        column: x => x.IDUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IDUsuario");
                });

            migrationBuilder.CreateTable(
                name: "DetallesDePedidos",
                columns: table => new
                {
                    IDDetalleDePedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IDPedido = table.Column<int>(type: "int", nullable: false),
                    PedidoIDPedido = table.Column<int>(type: "int", nullable: false),
                    IDProducto = table.Column<int>(type: "int", nullable: false),
                    ProductoIDProducto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesDePedidos", x => x.IDDetalleDePedido);
                    table.ForeignKey(
                        name: "FK_DetallesDePedidos_Pedidos_PedidoIDPedido",
                        column: x => x.PedidoIDPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IDPedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesDePedidos_Productos_ProductoIDProducto",
                        column: x => x.ProductoIDProducto,
                        principalTable: "Productos",
                        principalColumn: "IDProducto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialesDeEstados",
                columns: table => new
                {
                    IDHistorialEstados = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha_hora_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_hora_fin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IDUsuario = table.Column<int>(type: "int", nullable: false),
                    IDPedido = table.Column<int>(type: "int", nullable: false),
                    IDEstadoDePedido = table.Column<int>(type: "int", nullable: false),
                    UsuarioIDUsuario = table.Column<int>(type: "int", nullable: false),
                    PedidoIDPedido = table.Column<int>(type: "int", nullable: false),
                    EstadoDePedidoIDEstadoDePedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialesDeEstados", x => x.IDHistorialEstados);
                    table.ForeignKey(
                        name: "FK_HistorialesDeEstados_EstadosDePedidos_EstadoDePedidoIDEstadoDePedido",
                        column: x => x.EstadoDePedidoIDEstadoDePedido,
                        principalTable: "EstadosDePedidos",
                        principalColumn: "IDEstadoDePedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialesDeEstados_Pedidos_PedidoIDPedido",
                        column: x => x.PedidoIDPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IDPedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialesDeEstados_Usuarios_UsuarioIDUsuario",
                        column: x => x.UsuarioIDUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IDUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntentosDeEntregas",
                columns: table => new
                {
                    IDIntentoDeEntrega = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaDeIntento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RazonDeFallo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    URL_Foto_Verificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hora = table.Column<TimeSpan>(type: "time", nullable: false),
                    IDUsuario = table.Column<int>(type: "int", nullable: false),
                    IDPedido = table.Column<int>(type: "int", nullable: false),
                    UsuarioIDUsuario = table.Column<int>(type: "int", nullable: false),
                    PedidoIDPedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentosDeEntregas", x => x.IDIntentoDeEntrega);
                    table.ForeignKey(
                        name: "FK_IntentosDeEntregas_Pedidos_PedidoIDPedido",
                        column: x => x.PedidoIDPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IDPedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntentosDeEntregas_Usuarios_UsuarioIDUsuario",
                        column: x => x.UsuarioIDUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IDUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_LocalidadIDLocalidad",
                table: "Clientes",
                column: "LocalidadIDLocalidad");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDePedidos_PedidoIDPedido",
                table: "DetallesDePedidos",
                column: "PedidoIDPedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDePedidos_ProductoIDProducto",
                table: "DetallesDePedidos",
                column: "ProductoIDProducto");

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
                name: "IX_IntentosDeEntregas_PedidoIDPedido",
                table: "IntentosDeEntregas",
                column: "PedidoIDPedido");

            migrationBuilder.CreateIndex(
                name: "IX_IntentosDeEntregas_UsuarioIDUsuario",
                table: "IntentosDeEntregas",
                column: "UsuarioIDUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IDCliente",
                table: "Pedidos",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IDEstadoDePedido",
                table: "Pedidos",
                column: "IDEstadoDePedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IDSucursal",
                table: "Pedidos",
                column: "IDSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IDUsuario",
                table: "Pedidos",
                column: "IDUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_SucursalIDSucursal",
                table: "Usuarios",
                column: "SucursalIDSucursal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesDePedidos");

            migrationBuilder.DropTable(
                name: "HistorialesDeEstados");

            migrationBuilder.DropTable(
                name: "IntentosDeEntregas");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "EstadosDePedidos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Localidades");

            migrationBuilder.DropTable(
                name: "Sucursales");
        }
    }
}
