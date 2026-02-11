﻿using Back.Data;
using Back.Models;
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
            // BORRA y RECREA la base de datos siempre que arranca (para desarrollo/testing)
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // 1. MOTIVOS DE CANCELACIÓN (Nuevos requerimientos)
            context.MotivosCancelacion.AddRange(
                new MotivoCancelacion { Nombre = "Arrepentimiento" },
                new MotivoCancelacion { Nombre = "Falta de stock" },
                new MotivoCancelacion { Nombre = "Error en el pago" },
                new MotivoCancelacion { Nombre = "Dirección incorrecta" }
            );
            context.SaveChanges();

            // 1. ESTADOS
            context.EstadosDePedidos.AddRange(
                new EstadoDePedido { NombreEstado = "Sin preparar", motivo_cancelacion = "N/A" },      // ID 1
                new EstadoDePedido { NombreEstado = "Preparar pedido", motivo_cancelacion = "N/A" },   // ID 2
                new EstadoDePedido { NombreEstado = "Demorado", motivo_cancelacion = "N/A" },          // ID 3
                new EstadoDePedido { NombreEstado = "Listo para despachar", motivo_cancelacion = "N/A" }, // ID 4
                new EstadoDePedido { NombreEstado = "Despachando", motivo_cancelacion = "N/A" },       // ID 5
                new EstadoDePedido { NombreEstado = "En camino", motivo_cancelacion = "N/A" },         // ID 6
                new EstadoDePedido { NombreEstado = "Entregado", motivo_cancelacion = "N/A" },         // ID 7
                new EstadoDePedido { NombreEstado = "Entrega fallida", motivo_cancelacion = "N/A" },
                new EstadoDePedido { NombreEstado = "Cancelado", motivo_cancelacion = "Stock" }
            );
            context.SaveChanges();

            // 2. LOCALIDADES Y BARRIOS 
            var cordoba = new Localidad { Ciudad = "Córdoba", Provincia = "Córdoba", CodigoPostal = "5000" };
            context.Localidades.Add(cordoba);
            context.SaveChanges();
            context.Barrios.Add(new Barrio { Nombre = "Nueva Córdoba", IDLocalidad = cordoba.IDLocalidad });
            context.SaveChanges();

            // 3. SUCURSALES Y PRODUCTOS 
            context.Sucursales.Add(new Sucursal { NombreSucursal = "Farmacia Centro", Dirección = "Av. Colon 123", Teléfono = "3514445566" });
            context.SaveChanges();

            context.Productos.AddRange(
                new Producto { NombreProducto = "Amoxidal 500", Categoria = "Farmacia", CantidadProducto = 50, PrecioProducto = 1500.50m },
                new Producto { NombreProducto = "Ibuprofeno 600", Categoria = "Venta Libre", CantidadProducto = 100, PrecioProducto = 800.00m }
            );
            context.SaveChanges();

            // 4. USUARIOS
            var suc = context.Sucursales.First();
            context.Usuarios.AddRange(
                new Usuario { Nombre = "Admin", UsuarioNombre = "admin", Contraseña = "123", Rol = "Administrador", IDSucursal = suc.IDSucursal, Mail = "a@a.com" },
                new Usuario { Nombre = "Pepe", UsuarioNombre = "operario", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "o@o.com" },
                new Usuario { Nombre = "Carlos", UsuarioNombre = "cadete", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "c@c.com" }
            );
            context.SaveChanges();

            // 5. CLIENTES (con mail correcto)
            context.Clientes.Add(new Cliente
            {
                Nombre = "Juan",
                Apellido = "Perez",
                DNI = "30123456",
                IDBarrio = context.Barrios.First().IDBarrio,
                IDLocalidad = context.Localidades.First().IDLocalidad,
                Direccion = "Belgrano 800",
                Mail = "agustina.allende457@gmail.com"
            });
            context.SaveChanges();

            // --- CARGA DE PEDIDOS ---
            var cliente = context.Clientes.First();
            var prod = context.Productos.First();
            var loc = context.Localidades.First();
            var admin = context.Usuarios.First(u => u.Rol == "Administrador");
            var operario = context.Usuarios.First(u => u.Rol == "Operario");
            
            var stSinPreparar = context.EstadosDePedidos.First(e => e.NombreEstado == "Sin preparar");
            var stListo = context.EstadosDePedidos.First(e => e.NombreEstado == "Listo para despachar");

            // PEDIDO 1: SIN PREPARAR
            var p1 = new Pedido {
                Fecha = DateTime.Now, Total = 3000, EstadoActual = "Sin preparar",
                IDCliente = cliente.IDCliente, IDEstadoDePedido = stSinPreparar.IDEstadoDePedido,
                IDUsuario = admin.IDUsuario, IDSucursal = suc.IDSucursal, IDLocalidad = loc.IDLocalidad,
                DireccionEntrega = cliente.Direccion
            };
            context.Pedidos.Add(p1);

            // NUEVO PEDIDO 3: LISTO PARA DESPACHAR (Para que lo vea el Cadete)
            var p3 = new Pedido {
                Fecha = DateTime.Now.AddHours(-3), 
                Total = 4500, 
                EstadoActual = "Listo para despachar",
                IDCliente = cliente.IDCliente, 
                IDEstadoDePedido = stListo.IDEstadoDePedido, // ID 4
                IDUsuario = operario.IDUsuario, // Lo preparó Pepe
                IDSucursal = suc.IDSucursal, 
                IDLocalidad = loc.IDLocalidad,
                DireccionEntrega = "Chacabuco 123",
                FechaEntregaEstimada = DateTime.Now.AddHours(1)
            };
            context.Pedidos.Add(p3);
            
            context.SaveChanges();

            // Detalles
            context.DetallesDePedidos.Add(new DetalleDePedido { IDPedido = p3.IDPedido, IDProducto = prod.IDProducto, Cantidad = 3, PrecioUnitario = prod.PrecioProducto });
            
            // Historial para que aparezca en trazabilidad
            context.HistorialesDeEstados.Add(new HistorialDeEstados {
                IDPedido = p3.IDPedido, IDEstadoDePedido = stListo.IDEstadoDePedido,
                IDUsuario = operario.IDUsuario, fecha_hora_inicio = DateTime.Now.AddMinutes(-20),
                Observaciones = "Preparación terminada"
            });

            context.SaveChanges();

            Console.WriteLine("--- Semillado completo: Pedido 'Listo para despachar' (ID 4) disponible ---");
        }
    }
}