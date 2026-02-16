﻿using Back.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Back.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // BORRA y RECREA la base de datos siempre que arranca (para desarrollo/testing)
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
                new EstadoDePedido { NombreEstado = "Sin preparar", motivo_cancelacion = "N/A" },      
                new EstadoDePedido { NombreEstado = "Preparar pedido", motivo_cancelacion = "N/A" },   
                new EstadoDePedido { NombreEstado = "Demorado", motivo_cancelacion = "N/A" },          
                new EstadoDePedido { NombreEstado = "Listo para despachar", motivo_cancelacion = "N/A" }, 
                new EstadoDePedido { NombreEstado = "Despachando", motivo_cancelacion = "N/A" },       
                new EstadoDePedido { NombreEstado = "En camino", motivo_cancelacion = "N/A" },         
                new EstadoDePedido { NombreEstado = "Entregado", motivo_cancelacion = "N/A" },         
                new EstadoDePedido { NombreEstado = "Entrega fallida", motivo_cancelacion = "N/A" },
                new EstadoDePedido { NombreEstado = "Cancelado", motivo_cancelacion = "Arrepentimiento" }
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

            // 4.1 PRODUCTOS (sin ID explícito)
            var prod1 = new Producto { NombreProducto = "EDP Balance By Dadatina", Descripcion = "70ml, Dadatina", Categoria = "Perfumeria", CantidadProducto = 50, PrecioProducto = 47900m };
            var prod2 = new Producto { NombreProducto = "Boos Intense Black EDP", Descripcion = "90ml, Boos", Categoria = "Perfumeria", CantidadProducto = 100, PrecioProducto = 52927m };
            var prod582 = new Producto { NombreProducto = "Oneblade Face+Body", Descripcion = "Philips QP2824", Categoria = "Electro", CantidadProducto = 30, PrecioProducto = 110932.79m };
            var prod584 = new Producto { NombreProducto = "Planchita Pelo Bellissima", Descripcion = "Ceramic Long Plates", Categoria = "Electro", CantidadProducto = 20, PrecioProducto = 75199m };

            context.Productos.AddRange(prod1, prod2, prod582, prod584);
            context.SaveChanges();

            // 5. USUARIOS
            context.Usuarios.AddRange(
                new Usuario { Nombre = "Admin", UsuarioNombre = "admin", Contraseña = "123", Rol = "Administrador", IDSucursal = suc.IDSucursal, Mail = "a@a.com" },
                new Usuario { Nombre = "Pepe", UsuarioNombre = "operario", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "o@o.com" },
                new Usuario { Nombre = "Carlos", UsuarioNombre = "cadete", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "c@c.com" }
            );
            context.SaveChanges();

            // 6. CLIENTES
            var cliente = new Cliente
            {
                Nombre = "Juan",
                Apellido = "Perez",
                DNI = "30123456",
                IDBarrio = context.Barrios.First().IDBarrio,
                IDLocalidad = context.Localidades.First().IDLocalidad,
                Direccion = "Belgrano 800",
                Mail = "agustina.allende457@gmail.com"
            };
            context.Clientes.Add(cliente);
            context.SaveChanges();

            // --- CARGA DE PEDIDOS ---
            var admin = context.Usuarios.First(u => u.Rol == "Administrador");
            var operario = context.Usuarios.First(u => u.Rol == "Operario");
            var loc = context.Localidades.First();
            var stSinPreparar = context.EstadosDePedidos.First(e => e.NombreEstado == "Sin preparar");
            var stListo = context.EstadosDePedidos.First(e => e.NombreEstado == "Listo para despachar");

            // PEDIDO 1: SIN PREPARAR (Con perfume Dadatina)
            var p1 = new Pedido {
                Fecha = DateTime.Now, 
                Total = prod1.PrecioProducto, 
                EstadoActual = "Sin preparar",
                IDCliente = cliente.IDCliente, 
                IDEstadoDePedido = stSinPreparar.IDEstadoDePedido,
                IDUsuario = admin.IDUsuario, 
                IDSucursal = suc.IDSucursal, 
                IDLocalidad = loc.IDLocalidad,
                DireccionEntrega = cliente.Direccion
            };
            context.Pedidos.Add(p1);
            context.SaveChanges();

            context.DetallesDePedidos.Add(new DetalleDePedido { 
                IDPedido = p1.IDPedido, 
                IDProducto = prod1.IDProducto, 
                Cantidad = 1, 
                PrecioUnitario = prod1.PrecioProducto 
            });

            // Historial para el pedido 1
            context.HistorialesDeEstados.Add(new HistorialDeEstados {
                IDPedido = p1.IDPedido,
                IDEstadoDePedido = stSinPreparar.IDEstadoDePedido,
                IDUsuario = admin.IDUsuario,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = "Pedido creado en estado inicial"
            });

            // PEDIDO 2: LISTO PARA DESPACHAR (Con Oneblade y Boos)
            var p2 = new Pedido {
                Fecha = DateTime.Now.AddHours(-3), 
                Total = prod582.PrecioProducto + prod2.PrecioProducto, 
                EstadoActual = "Listo para despachar",
                IDCliente = cliente.IDCliente, 
                IDEstadoDePedido = stListo.IDEstadoDePedido,
                IDUsuario = operario.IDUsuario,
                IDSucursal = suc.IDSucursal, 
                IDLocalidad = loc.IDLocalidad,
                DireccionEntrega = "Chacabuco 123",
                FechaEntregaEstimada = DateTime.Now.AddHours(1)
            };
            context.Pedidos.Add(p2);
            context.SaveChanges();

            context.DetallesDePedidos.AddRange(
                new DetalleDePedido { IDPedido = p2.IDPedido, IDProducto = prod582.IDProducto, Cantidad = 1, PrecioUnitario = prod582.PrecioProducto },
                new DetalleDePedido { IDPedido = p2.IDPedido, IDProducto = prod2.IDProducto, Cantidad = 1, PrecioUnitario = prod2.PrecioProducto }
            );
            
            // Historial para el pedido 2
            context.HistorialesDeEstados.Add(new HistorialDeEstados {
                IDPedido = p2.IDPedido, 
                IDEstadoDePedido = stListo.IDEstadoDePedido,
                IDUsuario = operario.IDUsuario, 
                fecha_hora_inicio = DateTime.Now.AddMinutes(-20),
                Observaciones = "Preparación terminada con productos de perfumería"
            });

            context.SaveChanges();

            Console.WriteLine("--- Semillado completo: Pedidos con productos del catálogo cargados ---");
        }
    }
}

