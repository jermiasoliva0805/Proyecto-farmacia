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
                new EstadoDePedido { NombreEstado = "Preparar pedido", motivo_cancelacion = "N/A" },    // 2 (Inicio Armado)
                new EstadoDePedido { NombreEstado = "Demorado", motivo_cancelacion = "N/A" },           // 3
                new EstadoDePedido { NombreEstado = "Listo para despachar", motivo_cancelacion = "N/A" }, // 4 (Fin Armado)
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
            context.Barrios.Add(new Barrio { Nombre = "Nueva Córdoba", IDLocalidad = cordoba.IDLocalidad });
            context.SaveChanges();

            // 4. SUCURSALES
            var suc = new Sucursal { NombreSucursal = "Farmacia Centro", Dirección = "Av. Colon 123", Teléfono = "3514445566" };
            context.Sucursales.Add(suc);
            context.SaveChanges();

            // 4.1 PRODUCTOS
            var prod1 = new Producto { NombreProducto = "EDP Balance By Dadatina", Descripcion = "70ml, Dadatina", Categoria = "Perfumeria", CantidadProducto = 50, PrecioProducto = 47900m };
            var prod2 = new Producto { NombreProducto = "Boos Intense Black EDP", Descripcion = "90ml, Boos", Categoria = "Perfumeria", CantidadProducto = 100, PrecioProducto = 52927m };
            var prod582 = new Producto { NombreProducto = "Oneblade Face+Body", Descripcion = "Philips QP2824", Categoria = "Electro", CantidadProducto = 30, PrecioProducto = 110932.79m };

            context.Productos.AddRange(prod1, prod2, prod582);
            context.SaveChanges();

            // 5. USUARIOS
            var opAna = new Usuario { Nombre = "Ana", Apellido = "Lopez", UsuarioNombre = "ana", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "ana@test.com" };
            var opLuis = new Usuario { Nombre = "Luis", Apellido = "Gomez", UsuarioNombre = "luis", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "luis@test.com" };
            var opMarta = new Usuario { Nombre = "Marta", Apellido = "Sosa", UsuarioNombre = "marta", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "marta@test.com" };

            context.Usuarios.AddRange(
                new Usuario { Nombre = "Admin", UsuarioNombre = "admin", Contraseña = "123", Rol = "Administrador", IDSucursal = suc.IDSucursal, Mail = "a@a.com" },
                opAna, opLuis, opMarta,
                new Usuario { Nombre = "Carlos", UsuarioNombre = "cadete", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "c@c.com" }
            );
            context.SaveChanges();

            // 6. CLIENTES
            var cliente = new Cliente { Nombre = "Juan", Apellido = "Perez", DNI = "30123456", IDBarrio = context.Barrios.First().IDBarrio, IDLocalidad = context.Localidades.First().IDLocalidad, Direccion = "Belgrano 800", Mail = "test@gmail.com" };
            context.Clientes.Add(cliente);
            context.SaveChanges();

            // --- 7. DATA PARA REPORTES (Simulación de Tiempos de Armado) ---
            var usuariosReporte = new List<Usuario> { opAna, opLuis, opMarta };
            var random = new Random();

            foreach (var op in usuariosReporte)
            {
                // Le creamos 5 pedidos a cada uno para que el reporte tenga volumen
                for (int i = 0; i < 5; i++)
                {
                    var p = new Pedido {
                        Fecha = DateTime.Now.AddDays(-random.Next(1, 5)),
                        Total = 15000,
                        EstadoActual = "Listo para despachar",
                        IDCliente = cliente.IDCliente,
                        IDEstadoDePedido = 4, // Listo para despachar
                        IDUsuario = op.IDUsuario,
                        IDSucursal = suc.IDSucursal,
                        IDLocalidad = cordoba.IDLocalidad,
                        DireccionEntrega = "Calle Falsa 123"
                    };
                    context.Pedidos.Add(p);
                    context.SaveChanges();

                    // Definimos minutos según el operario para ver diferencias en el gráfico
                    // Ana será muy rápida (15-25 min), Luis será lento (35-50 min), Marta equilibrada.
                    int minutosArmado = op.Nombre == "Ana" ? random.Next(15, 25) : 
                                    op.Nombre == "Luis" ? random.Next(35, 55) : random.Next(25, 35);

                    var fechaInicio = p.Fecha.AddMinutes(-minutosArmado);

                    // Historial 1: Inicio (Estado 2 - Preparar pedido)
                    context.HistorialesDeEstados.Add(new HistorialDeEstados {
                        IDPedido = p.IDPedido,
                        IDEstadoDePedido = 2,
                        IDUsuario = op.IDUsuario,
                        fecha_hora_inicio = fechaInicio,
                        Observaciones = "Empezó a preparar"
                    });

                    // Historial 2: Fin (Estado 4 - Listo para despachar)
                    context.HistorialesDeEstados.Add(new HistorialDeEstados {
                        IDPedido = p.IDPedido,
                        IDEstadoDePedido = 4,
                        IDUsuario = op.IDUsuario,
                        fecha_hora_inicio = fechaInicio.AddMinutes(minutosArmado),
                        Observaciones = "Terminó de preparar"
                    });
                }
            }

            context.SaveChanges();
            Console.WriteLine("--- Semillado completo: Data de eficiencia generada ---");
        }
    }
}