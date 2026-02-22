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
            
            // ✅ CADETES - AGREGADOS
            var cadete1 = new Usuario { Nombre = "Carlos", Apellido = "Martinez", UsuarioNombre = "carlos", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "carlos@test.com" };
            var cadete2 = new Usuario { Nombre = "Pedro", Apellido = "Romero", UsuarioNombre = "pedro", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "pedro@test.com" };
            var cadete3 = new Usuario { Nombre = "Sofia", Apellido = "Garcia", UsuarioNombre = "sofia", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "sofia@test.com" };

            context.Usuarios.AddRange(
                new Usuario { Nombre = "Admin", Apellido = "User", UsuarioNombre = "admin", Contraseña = "123", Rol = "Administrador", IDSucursal = suc.IDSucursal, Mail = "a@a.com" },
                opAna, opLuis, opMarta,
                cadete1, cadete2, cadete3
            );
            context.SaveChanges();

            // 6. CLIENTES
            var cliente1 = new Cliente { Nombre = "Juan", Apellido = "Perez", DNI = "30123456", IDBarrio = context.Barrios.First().IDBarrio, IDLocalidad = context.Localidades.First().IDLocalidad, Direccion = "Belgrano 800", Mail = "juan@gmail.com", Telefono = "3512345678" };
            var cliente2 = new Cliente { Nombre = "Maria", Apellido = "Gonzalez", DNI = "32654321", IDBarrio = context.Barrios.First().IDBarrio, IDLocalidad = context.Localidades.First().IDLocalidad, Direccion = "San Martin 500", Mail = "maria@gmail.com", Telefono = "3517654321" };
            context.Clientes.AddRange(cliente1, cliente2);
            context.SaveChanges();

            // 7. PEDIDOS PARA OPERARIOS (Armado)
            var usuariosOperarios = new List<Usuario> { opAna, opLuis, opMarta };
            var random = new Random();

            foreach (var op in usuariosOperarios)
            {
                for (int i = 0; i < 5; i++)
                {
                    var p = new Pedido
                    {
                        Fecha = DateTime.Now.AddDays(-random.Next(1, 5)),
                        Total = 15000,
                        EstadoActual = "Listo para despachar",
                        IDCliente = cliente1.IDCliente,
                        IDEstadoDePedido = 4,
                        IDUsuario = op.IDUsuario,
                        IDSucursal = suc.IDSucursal,
                        IDLocalidad = cordoba.IDLocalidad,
                        DireccionEntrega = "Calle Falsa 123"
                    };
                    context.Pedidos.Add(p);
                    context.SaveChanges();

                    int minutosArmado = op.Nombre == "Ana" ? random.Next(15, 25) : 
                                    op.Nombre == "Luis" ? random.Next(35, 55) : random.Next(25, 35);

                    var fechaInicio = p.Fecha.AddMinutes(-minutosArmado);

                    context.HistorialesDeEstados.AddRange(
                        new HistorialDeEstados
                        {
                            IDPedido = p.IDPedido,
                            IDEstadoDePedido = 2,
                            IDUsuario = op.IDUsuario,
                            fecha_hora_inicio = fechaInicio,
                            Observaciones = "Empezó a preparar"
                        },
                        new HistorialDeEstados
                        {
                            IDPedido = p.IDPedido,
                            IDEstadoDePedido = 4,
                            IDUsuario = op.IDUsuario,
                            fecha_hora_inicio = fechaInicio.AddMinutes(minutosArmado),
                            Observaciones = "Terminó de preparar"
                        }
                    );
                }
            }
            context.SaveChanges();

            // ✅ 8. PEDIDOS PARA CADETES (Entregas)
            var usuariosCadetes = new List<Usuario> { cadete1, cadete2, cadete3 };

            foreach (var cadete in usuariosCadetes)
            {
                for (int i = 0; i < 6; i++)
                {
                    var estado = random.Next(1, 101);
                    int idEstado;
                    
                    if (estado <= 60) idEstado = 7;        // 60% Entregado
                    else if (estado <= 80) idEstado = 8;   // 20% Fallo
                    else idEstado = 9;                     // 20% Cancelado

                    var p = new Pedido
                    {
                        Fecha = DateTime.Now.AddDays(-random.Next(1, 7)),
                        Total = (decimal)random.Next(10000, 100000),
                        EstadoActual = idEstado == 7 ? "Entregado" : idEstado == 8 ? "Entrega fallida" : "Cancelado",
                        IDCliente = random.Next(1, 3) == 1 ? cliente1.IDCliente : cliente2.IDCliente,
                        IDEstadoDePedido = idEstado,
                        IDUsuario = cadete.IDUsuario,
                        IDSucursal = suc.IDSucursal,
                        IDLocalidad = cordoba.IDLocalidad,
                        DireccionEntrega = "Direccion de entrega " + random.Next(1, 100),
                        FechaEntregaEstimada = DateTime.Now.AddDays(1),
                        FechaEntregaReal = idEstado == 7 ? DateTime.Now.AddHours(-random.Next(1, 24)) : null,
                        HoraEntregaEstimada = new TimeSpan(14, 0, 0),
                        IntentosEntregaFallida = idEstado == 8 ? random.Next(1, 3) : (idEstado == 9 ? 3 : 0)
                    };
                    context.Pedidos.Add(p);
                    context.SaveChanges();

                    // Historial para cadete
                    context.HistorialesDeEstados.Add(new HistorialDeEstados
                    {
                        IDPedido = p.IDPedido,
                        IDEstadoDePedido = idEstado,
                        IDUsuario = cadete.IDUsuario,
                        fecha_hora_inicio = p.Fecha,
                        Observaciones = idEstado == 7 ? "Entrega completada" : 
                                       idEstado == 8 ? "Intento fallido" : "Pedido cancelado"
                    });
                }
            }
            context.SaveChanges();

            Console.WriteLine("✅ --- Semillado completo: Data de reportes generada ---");
        }
    }
}