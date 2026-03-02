﻿using Back.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Back.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // 1. MOTIVOS DE CANCELACIÓN
            context.MotivosCancelacion.AddRange(
                new MotivoCancelacion { Nombre = "Arrepentimiento" },
                new MotivoCancelacion { Nombre = "Falta de stock" },
                new MotivoCancelacion { Nombre = "Error en el pago" },
                new MotivoCancelacion { Nombre = "Dirección incorrecta" }
            );
            context.SaveChanges();

            // 2. ESTADOS DE PEDIDO
            context.EstadosDePedidos.AddRange(
                new EstadoDePedido { NombreEstado = "Sin preparar", motivo_cancelacion = "N/A" },      // 1
                new EstadoDePedido { NombreEstado = "Preparar pedido", motivo_cancelacion = "N/A" },    // 2
                new EstadoDePedido { NombreEstado = "Demorado", motivo_cancelacion = "N/A" },           // 3
                new EstadoDePedido { NombreEstado = "Listo para despachar", motivo_cancelacion = "N/A" }, // 4
                new EstadoDePedido { NombreEstado = "Despachando", motivo_cancelacion = "N/A" },        // 5
                new EstadoDePedido { NombreEstado = "En camino", motivo_cancelacion = "N/A" },          // 6
                new EstadoDePedido { NombreEstado = "Entregado", motivo_cancelacion = "N/A" },          // 7
                new EstadoDePedido { NombreEstado = "Entrega fallida", motivo_cancelacion = "N/A" },    // 8
                new EstadoDePedido { NombreEstado = "Cancelado", motivo_cancelacion = "Arrepentimiento" } // 9
            );
            context.SaveChanges();

            // 3. LOCALIDADES Y BARRIOS 
            var cordoba = new Localidad { Ciudad = "Córdoba", Provincia = "Córdoba", CodigoPostal = "5000" };
            context.Localidades.Add(cordoba);
            context.SaveChanges();
            var barrioGral = new Barrio { Nombre = "Nueva Córdoba", IDLocalidad = cordoba.IDLocalidad };
            context.Barrios.Add(barrioGral);
            context.SaveChanges();

            // 4. SUCURSALES
            var suc = new Sucursal { NombreSucursal = "Farmacia General Paz Centro", Dirección = "Av. Colon 123", Teléfono = "3514445566" };
            context.Sucursales.Add(suc);
            context.SaveChanges();

            // 4.1 CATÁLOGO AMPLIADO (12 Productos de diversas categorías)
            var listaProductos = new List<Producto>
            {
                // PERFUMERÍA
                new Producto { NombreProducto = "EDP Balance By Dadatina", Descripcion = "70ml, Dadatina", Categoria = "Perfumeria", CantidadProducto = 50, PrecioProducto = 47900m },
                new Producto { NombreProducto = "Boos Intense Black EDP", Descripcion = "90ml, fragancia masculina", Categoria = "Perfumeria", CantidadProducto = 100, PrecioProducto = 52927m },
                new Producto { NombreProducto = "Shakira Amarillo EDP", Descripcion = "80ml, fragancia femenina", Categoria = "Perfumeria", CantidadProducto = 30, PrecioProducto = 45045m },
                
                // ELECTRO
                new Producto { NombreProducto = "Oneblade Face+Body", Descripcion = "Philips QP2824 - Afeitadora", Categoria = "Electro", CantidadProducto = 30, PrecioProducto = 110932.79m },
                new Producto { NombreProducto = "Nebulizador Pistón", Descripcion = "ASPEN Nbb02-A-50 Silencioso", Categoria = "Electro", CantidadProducto = 20, PrecioProducto = 80330.58m },
                new Producto { NombreProducto = "Tensiometro Aneroide", Descripcion = "FEMMTO Kit con Estetoscopio", Categoria = "Electro", CantidadProducto = 25, PrecioProducto = 42274.33m },
                new Producto { NombreProducto = "Planchita Pelo Bellissima", Descripcion = "Ceramic Long Plates", Categoria = "Electro", CantidadProducto = 15, PrecioProducto = 75199m },

                // CUIDADO PERSONAL / DERMOCOSMÉTICA
                new Producto { NombreProducto = "Protector Solar Eucerin", Descripcion = "Toque Seco FPS 50+ 50ml", Categoria = "Cuidado Personal", CantidadProducto = 60, PrecioProducto = 28500.50m },
                new Producto { NombreProducto = "Serum Effaclar La Roche", Descripcion = "Peeling diario 30ml", Categoria = "Cuidado Personal", CantidadProducto = 40, PrecioProducto = 41200m },

                // SALUD / FARMACIA
                new Producto { NombreProducto = "Termómetro IR Aspen", Descripcion = "Infrarrojo TS8 4 en 1", Categoria = "Salud", CantidadProducto = 35, PrecioProducto = 42798.99m },
                new Producto { NombreProducto = "Tiras Accu Chek Guide", Descripcion = "Caja x 25 unidades", Categoria = "Salud", CantidadProducto = 100, PrecioProducto = 73521m },
                new Producto { NombreProducto = "Alcohol en Gel 500ml", Descripcion = "Antibacterial con dosificador", Categoria = "Salud", CantidadProducto = 200, PrecioProducto = 3500m }
            };

            context.Productos.AddRange(listaProductos);
            context.SaveChanges();

            // 5. USUARIOS
            var userAdmin = new Usuario { Nombre = "Admin", Apellido = "Sistema", UsuarioNombre = "admin", Contraseña = "123", Rol = "Administrador", IDSucursal = suc.IDSucursal, Mail= "admin@farmacia.com" };
            var opAna = new Usuario { Nombre = "Ana", Apellido = "Lopez", UsuarioNombre = "anaLop", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "ana@test.com" };
            var cadete1 = new Usuario { Nombre = "Carlos", Apellido = "Martinez", UsuarioNombre = "carlos", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "carlos@test.com" };

            context.Usuarios.AddRange(userAdmin, opAna, cadete1);
            context.SaveChanges();
            
            // 6. CLIENTES
            var cliente1 = new Cliente { Nombre = "Juan", Apellido = "Perez", DNI = "30123456", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Belgrano 800", Mail = "juan@gmail.com", Telefono = "3512345678" };
            var cliente2 = new Cliente { Nombre = "Maria", Apellido = "Gonzalez", DNI = "32654321", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "San Martin 500", Mail = "maria@gmail.com", Telefono = "3517654321" };
            context.Clientes.AddRange(cliente1, cliente2);
            context.SaveChanges();

            // 7. GENERACIÓN AUTOMÁTICA DE PEDIDOS
            var random = new Random();

            // Pedidos Pendientes
            for (int i = 0; i < 6; i++)
            {
                var p = CrearPedidoBase(context, userAdmin.IDUsuario, 1, "Sin preparar", cliente2.IDCliente, suc.IDSucursal, cordoba.IDLocalidad);
                AsignarProductosAPedido(context, p, listaProductos, random);
            }

            // Pedidos en Proceso (Operario)
            for (int i = 0; i < 5; i++)
            {
                var p = CrearPedidoBase(context, opAna.IDUsuario, 2, "Preparar pedido", cliente1.IDCliente, suc.IDSucursal, cordoba.IDLocalidad);
                AsignarProductosAPedido(context, p, listaProductos, random);
            }

            // Pedidos Entregados (Cadete)
            for (int i = 0; i < 4; i++)
            {
                var p = CrearPedidoBase(context, cadete1.IDUsuario, 7, "Entregado", cliente1.IDCliente, suc.IDSucursal, cordoba.IDLocalidad);
                p.FechaEntregaReal = DateTime.Now.AddHours(-1);
                AsignarProductosAPedido(context, p, listaProductos, random);
            }

            context.SaveChanges();
        }

        // Métodos auxiliares para limpieza del código
        private static Pedido CrearPedidoBase(AppDbContext context, int userId, int estadoId, string estadoNombre, int clienteId, int sucId, int locId)
        {
            var p = new Pedido {
                Fecha = DateTime.Now.AddMinutes(-new Random().Next(60, 1000)),
                Total = 0,
                EstadoActual = estadoNombre,
                IDCliente = clienteId,
                IDEstadoDePedido = estadoId,
                IDUsuario = userId, 
                IDSucursal = sucId,
                IDLocalidad = locId,
                DireccionEntrega = "Calle Ficticia " + new Random().Next(1, 999)
            };
            context.Pedidos.Add(p);
            context.SaveChanges();
            
            context.HistorialesDeEstados.Add(new HistorialDeEstados {
                IDPedido = p.IDPedido, IDEstadoDePedido = estadoId, IDUsuario = userId,
                fecha_hora_inicio = p.Fecha, Observaciones = "Generado por Seed"
            });
            
            return p;
        }

        private static void AsignarProductosAPedido(AppDbContext context, Pedido pedido, List<Producto> productos, Random rng)
        {
            int itemsCount = rng.Next(1, 5); 
            decimal acumulado = 0;

            // Seleccionar productos sin repetir en el mismo pedido
            var seleccionados = productos.OrderBy(x => rng.Next()).Take(itemsCount).ToList();

            foreach (var prod in seleccionados)
            {
                int cantidad = rng.Next(1, 3);
                context.DetallesDePedidos.Add(new DetalleDePedido {
                    IDPedido = pedido.IDPedido,
                    IDProducto = prod.IDProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = prod.PrecioProducto
                });
                acumulado += (prod.PrecioProducto * cantidad);
            }

            pedido.Total = acumulado;
            context.SaveChanges();
        }
    }
}