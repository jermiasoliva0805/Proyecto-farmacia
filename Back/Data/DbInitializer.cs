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
                new MotivoCancelacion { Nombre = "Arrepentimiento", Activo = true },
                new MotivoCancelacion { Nombre = "Falta de stock", Activo = true },
                new MotivoCancelacion { Nombre = "Error en el pago", Activo = true },
                new MotivoCancelacion { Nombre = "Dirección incorrecta", Activo = true }
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

            // 4.1 PRODUCTOS (Catálogo Ampliado)
            var listaProductos = new List<Producto>
            {
                new Producto { NombreProducto = "EDP Balance By Dadatina", Descripcion = "70ml, Dadatina", Categoria = "Perfumeria", CantidadProducto = 50, PrecioProducto = 47900m },
                new Producto { NombreProducto = "Boos Intense Black EDP", Descripcion = "90ml, fragancia masculina", Categoria = "Perfumeria", CantidadProducto = 100, PrecioProducto = 52927m },
                new Producto { NombreProducto = "Shakira Amarillo EDP", Descripcion = "80ml, fragancia femenina", Categoria = "Perfumeria", CantidadProducto = 30, PrecioProducto = 45045m },
                new Producto { NombreProducto = "Oneblade Face+Body", Descripcion = "Philips QP2824 - Afeitadora", Categoria = "Electro", CantidadProducto = 30, PrecioProducto = 110932.79m },
                new Producto { NombreProducto = "Nebulizador Pistón", Descripcion = "ASPEN Nbb02-A-50 Silencioso", Categoria = "Electro", CantidadProducto = 20, PrecioProducto = 80330.58m },
                new Producto { NombreProducto = "Tensiometro Aneroide", Descripcion = "FEMMTO Kit con Estetoscopio", Categoria = "Electro", CantidadProducto = 25, PrecioProducto = 42274.33m },
                new Producto { NombreProducto = "Planchita Pelo Bellissima", Descripcion = "Ceramic Long Plates", Categoria = "Electro", CantidadProducto = 15, PrecioProducto = 75199m },
                new Producto { NombreProducto = "Protector Solar Eucerin", Descripcion = "Toque Seco FPS 50+ 50ml", Categoria = "Cuidado Personal", CantidadProducto = 60, PrecioProducto = 28500.50m },
                new Producto { NombreProducto = "Serum Effaclar La Roche", Descripcion = "Peeling diario 30ml", Categoria = "Cuidado Personal", CantidadProducto = 40, PrecioProducto = 41200m },
                new Producto { NombreProducto = "Termómetro IR Aspen", Descripcion = "Infrarrojo TS8 4 en 1", Categoria = "Salud", CantidadProducto = 35, PrecioProducto = 42798.99m },
                new Producto { NombreProducto = "Tiras Accu Chek Guide", Descripcion = "Caja x 25 unidades", Categoria = "Salud", CantidadProducto = 100, PrecioProducto = 73521m },
                new Producto { NombreProducto = "Alcohol en Gel 500ml", Descripcion = "Antibacterial con dosificador", Categoria = "Salud", CantidadProducto = 200, PrecioProducto = 3500m }
            };
            context.Productos.AddRange(listaProductos);
            context.SaveChanges();

            // 5. USUARIOS
            var userAdmin = new Usuario { Nombre = "Admin", Apellido = "Sistema", UsuarioNombre = "admin", Contraseña = "123", Rol = "Administrador", IDSucursal = suc.IDSucursal, Mail= "admin@farmacia.com" };
            var opAna = new Usuario { Nombre = "Ana", Apellido = "Lopez", UsuarioNombre = "anaLop", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "ana@test.com" };
            var opLuis = new Usuario { Nombre = "Luis", Apellido = "Gomez", UsuarioNombre = "luis", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "luis@test.com" };
            var opMarta = new Usuario { Nombre = "Marta", Apellido = "Sosa", UsuarioNombre = "marta", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "marta@test.com" };
            var cadete1 = new Usuario { Nombre = "Carlos", Apellido = "Martinez", UsuarioNombre = "carlos", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "carlos@test.com" };
            var cadete2 = new Usuario { Nombre = "Pedro", Apellido = "Romero", UsuarioNombre = "pedro", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "pedro@test.com" };
            var cadete3 = new Usuario { Nombre = "Sofia", Apellido = "Garcia", UsuarioNombre = "sofia", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "sofia@test.com" };

            context.Usuarios.AddRange(userAdmin, opAna, opLuis, opMarta, cadete1, cadete2, cadete3);
            context.SaveChanges();

            // 6. CLIENTES (Ampliados a 10 para reportes detallados)
            var clientes = new List<Cliente>
            {
                new Cliente { Nombre = "Juan", Apellido = "Perez", DNI = "30123456", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Belgrano 800", Mail = "aguallende457@gmail.com", Telefono = "3512345678" },
                new Cliente { Nombre = "Maria", Apellido = "Gonzalez", DNI = "32654321", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "San Martin 500", Mail = "aguallende457@gmail.com", Telefono = "3517654321" },
                new Cliente { Nombre = "Carlos", Apellido = "Rodriguez", DNI = "31789456", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Av. Velez Sarsfield 1200", Mail = "aguallende457@gmail.com", Telefono = "3518765432" },
                new Cliente { Nombre = "Ana", Apellido = "Martinez", DNI = "33456789", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Calle Ituzaingo 450", Mail = "aguallende457@gmail.com", Telefono = "3519876543" },
                new Cliente { Nombre = "Roberto", Apellido = "Lopez", DNI = "34567890", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Avenida Colon 600", Mail = "aguallende457@gmail.com", Telefono = "3514444555" },
                new Cliente { Nombre = "Gabriela", Apellido = "Sanchez", DNI = "35678901", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Ruta Nacional 9 Km 10", Mail = "aguallende457@gmail.com", Telefono = "3516666777" },
                new Cliente { Nombre = "Fernando", Apellido = "Diaz", DNI = "36789012", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Calle Hipólito Irigoyen 800", Mail = "aguallende457@gmail.com", Telefono = "3517777888" },
                new Cliente { Nombre = "Alejandra", Apellido = "Torres", DNI = "37890123", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Paseo Sobremonte 350", Mail = "aguallende457@gmail.com", Telefono = "3518888999" },
                new Cliente { Nombre = "Jorge", Apellido = "Castro", DNI = "38901234", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Calle General Paz 950", Mail = "aguallende457@gmail.com", Telefono = "3519999000" },
                new Cliente { Nombre = "Patricia", Apellido = "Flores", DNI = "39012345", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Avenida Maipú 1100", Mail = "aguallende457@gmail.com", Telefono = "3510101010" }
            };
            context.Clientes.AddRange(clientes);
            context.SaveChanges();

            // 7. GENERACIÓN DE PEDIDOS PARA REPORTES
            var random = new Random();
            int[] diasRangos = { 1, 7, 30, 90 };

            // A. Pedidos Pendientes (Asignados al Admin por defecto) - para que haya algo "en proceso"
            for (int i = 0; i < 3; i++)
            {
                var p = CrearPedidoBase(context, userAdmin.IDUsuario, 1, "Sin preparar", clientes[0].IDCliente, suc.IDSucursal, cordoba.IDLocalidad);
                AsignarProductosAPedido(context, p, listaProductos, random);
            }

            // B. PEDIDOS PARA OPERARIOS - DISTRIBUIDOS EN DIFERENTES PERÍODOS
            var operarios = new List<Usuario> { opAna, opLuis, opMarta };
            
            foreach (int diasAtras in diasRangos)
            {
                foreach (var cliente in clientes)
                {
                    foreach (var op in operarios)
                    {
                        // Crear 1 pedido preparado por operario-cliente en cada rango de días
                        var baseDate = DateTime.Now.AddDays(-random.Next(1, diasAtras + 1));
                        
                        var p = new Pedido
                        {
                            Fecha = baseDate,
                            Total = 0,
                            EstadoActual = "Listo para despachar",
                            IDCliente = cliente.IDCliente,
                            IDEstadoDePedido = 4,
                            IDUsuario = op.IDUsuario,
                            IDSucursal = suc.IDSucursal,
                            IDLocalidad = cordoba.IDLocalidad,
                            DireccionEntrega = cliente.Direccion
                        };
                        context.Pedidos.Add(p);
                        context.SaveChanges();

                        // Crear transiciones de estado realistas (Estado 2 -> Estado 4)
                        var timeState2 = baseDate;
                        context.HistorialesDeEstados.Add(new HistorialDeEstados
                        {
                            IDPedido = p.IDPedido,
                            IDEstadoDePedido = 2,
                            IDUsuario = op.IDUsuario,
                            fecha_hora_inicio = timeState2,
                            Observaciones = "Pedido asignado a preparación"
                        });

                        // Tiempo variable de preparación (15-45 minutos)
                        var timeState4 = timeState2.AddMinutes(random.Next(15, 46));
                        context.HistorialesDeEstados.Add(new HistorialDeEstados
                        {
                            IDPedido = p.IDPedido,
                            IDEstadoDePedido = 4,
                            IDUsuario = op.IDUsuario,
                            fecha_hora_inicio = timeState4,
                            Observaciones = "Pedido completamente preparado"
                        });

                        AsignarProductosAPedido(context, p, listaProductos, random);
                    }
                }
            }

            // C. PEDIDOS ENTREGADOS POR CADETES - DISTRIBUIDOS EN DIFERENTES PERÍODOS
            var cadetes = new List<Usuario> { cadete1, cadete2, cadete3 };
            
            foreach (int diasAtras in diasRangos)
            {
                foreach (var cliente in clientes)
                {
                    foreach (var cadete in cadetes)
                    {
                        // Crear 1 pedido entregado por cliente-cadete en cada rango de días
                        var baseDate = DateTime.Now.AddDays(-random.Next(1, diasAtras + 1));
                        
                        var p = new Pedido
                        {
                            Fecha = baseDate,
                            Total = 0,
                            EstadoActual = "Entregado",
                            IDCliente = cliente.IDCliente,
                            IDEstadoDePedido = 7,
                            IDUsuario = cadete.IDUsuario,
                            IDSucursal = suc.IDSucursal,
                            IDLocalidad = cordoba.IDLocalidad,
                            DireccionEntrega = cliente.Direccion,
                            FechaEntregaReal = baseDate.AddHours(random.Next(2, 8))
                        };
                        context.Pedidos.Add(p);
                        context.SaveChanges();

                        // Crear transiciones de estado realistas para cadetes
                        var timeState5 = baseDate;
                        context.HistorialesDeEstados.Add(new HistorialDeEstados
                        {
                            IDPedido = p.IDPedido,
                            IDEstadoDePedido = 5,
                            IDUsuario = cadete.IDUsuario,
                            fecha_hora_inicio = timeState5,
                            Observaciones = "Pedido asignado a cadete para despacho"
                        });

                        var timeState6 = timeState5.AddMinutes(random.Next(10, 21));
                        context.HistorialesDeEstados.Add(new HistorialDeEstados
                        {
                            IDPedido = p.IDPedido,
                            IDEstadoDePedido = 6,
                            IDUsuario = cadete.IDUsuario,
                            fecha_hora_inicio = timeState6,
                            Observaciones = "Pedido en ruta de entrega"
                        });

                        var timeState7 = timeState6.AddMinutes(random.Next(20, 61));
                        context.HistorialesDeEstados.Add(new HistorialDeEstados
                        {
                            IDPedido = p.IDPedido,
                            IDEstadoDePedido = 7,
                            IDUsuario = cadete.IDUsuario,
                            fecha_hora_inicio = timeState7,
                            Observaciones = "Pedido entregado exitosamente"
                        });

                        AsignarProductosAPedido(context, p, listaProductos, random);
                    }
                }
            }

            context.SaveChanges();
            Console.WriteLine("✅ Semillado con 10 clientes y pedidos distribuidos en diferentes períodos.");
        }

        // --- MÉTODOS AUXILIARES ---
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
                fecha_hora_inicio = p.Fecha, Observaciones = "Generado por Seed con Productos"
            });
            return p;
        }

        private static void AsignarProductosAPedido(AppDbContext context, Pedido pedido, List<Producto> productos, Random rng)
        {
            int itemsCount = rng.Next(1, 4); 
            decimal acumulado = 0;
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