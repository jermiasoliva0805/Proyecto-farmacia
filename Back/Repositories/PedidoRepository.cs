using Back.Data;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _context;
        private const int MaxIntentosFallidos = 3;

        public PedidoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderSummaryDTO>> GetFilteredOrdersAsync(OrderFilterDTO filters)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EstadoDePedido)
                .Include(p => p.Usuario)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                string term = filters.Search.ToLower();
                query = query.Where(p =>
                    p.IDPedido.ToString().Contains(term) ||
                    (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(term)) ||
                    (p.Cliente != null && p.Cliente.Apellido.ToLower().Contains(term))
                );
            }

            if (filters.IDEstadoDePedido.HasValue && filters.IDEstadoDePedido.Value > 0)
            {
                query = query.Where(p => p.IDEstadoDePedido == filters.IDEstadoDePedido.Value);
            }

            if (filters.IDUsuario.HasValue && filters.IDUsuario.Value > 0)
            {
                query = query.Where(p => p.IDUsuario == filters.IDUsuario.Value);
            }

            if (filters.IDCliente.HasValue && filters.IDCliente.Value > 0)
            {
                query = query.Where(p => p.IDCliente == filters.IDCliente.Value);
            }

            if (filters.FechaDesde.HasValue)
            {
                var desde = filters.FechaDesde.Value.Date;
                query = query.Where(p => p.Fecha.Date >= desde);
            }

            if (filters.FechaHasta.HasValue)
            {
                var hasta = filters.FechaHasta.Value.Date;
                query = query.Where(p => p.Fecha.Date <= hasta);
            }

            return await query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new OrderSummaryDTO
                {
                    IDPedido = p.IDPedido,
                    Fecha = p.Fecha,
                    Total = p.Total,
                    IDEstadoDePedido = p.IDEstadoDePedido,
                    EstadoNombre = p.EstadoDePedido != null ? p.EstadoDePedido.NombreEstado : "Sin Estado",
                    ClienteNombre = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Sin Cliente",
                    ResponsableNombre = p.Usuario != null ? p.Usuario.Nombre : "Sin Asignar",
                    FechaEntregaEstimada = p.FechaEntregaEstimada,
                    FechaEntregaReal = p.FechaEntregaReal
                })
                .ToListAsync();
        }

        public async Task<Pedido?> GetByIdAsync(int idPedido)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EstadoDePedido)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                .Include(p => p.HistorialDeEstados)
                .FirstOrDefaultAsync(p => p.IDPedido == idPedido);
        }

        public async Task UpdateAsync(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarEstadoPedidoAsync(ChangeOrderStatusDTO datos)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.HistorialDeEstados)
                    .FirstOrDefaultAsync(p => p.IDPedido == datos.IDPedido);

                if (pedido == null) return false;

                // Usa Set<EstadoDePedido>() para acceder a la tabla de estados sin necesitar DbSet explícito
                var estados = _context.Set<EstadoDePedido>();

                // Resolver IDs por nombre para evitar desalineación
                var idEntregado = await estados
                    .Where(e => e.NombreEstado == "Entregado")
                    .Select(e => e.IDEstadoDePedido)
                    .FirstOrDefaultAsync();

                var idEntregaFallida = await estados
                    .Where(e => e.NombreEstado == "Entrega fallida")
                    .Select(e => e.IDEstadoDePedido)
                    .FirstOrDefaultAsync();

                var idCancelado = await estados
                    .Where(e => e.NombreEstado == "Cancelado")
                    .Select(e => e.IDEstadoDePedido)
                    .FirstOrDefaultAsync();

                // Si ya está final, no permitir cambios
                if (pedido.IDEstadoDePedido == idEntregado || pedido.IDEstadoDePedido == idCancelado)
                    return false;

                if (datos.IDNuevoEstado == idEntregaFallida)
                {
                    // Conteo null-safe de intentos previos de "Entrega fallida"
                    var fallasPrevias = pedido.HistorialDeEstados?.Count(h => h.IDEstadoDePedido == idEntregaFallida) ?? 0;
                    var siguienteIntento = fallasPrevias + 1;

                    if (siguienteIntento >= MaxIntentosFallidos)
                    {
                        pedido.IDEstadoDePedido = idCancelado;
                        pedido.EstadoActual = "Cancelado";
                        datos.Observaciones ??= "Cancelado automáticamente: superó el máximo de intentos de entrega (3).";
                        datos.MotivoCancelacion ??= "Superó el máximo de intentos de entrega (3).";
                    }
                    else
                    {
                        pedido.IDEstadoDePedido = idEntregaFallida;
                        pedido.EstadoActual = "Entrega fallida";
                        datos.Observaciones ??= $"Intento de entrega fallido #{siguienteIntento}.";
                    }
                }
                else if (datos.IDNuevoEstado == idEntregado)
                {
                    pedido.IDEstadoDePedido = idEntregado;
                    pedido.EstadoActual = "Entregado";
                    pedido.FechaEntregaReal = DateTime.Now;
                    datos.Observaciones ??= "Pedido entregado exitosamente.";
                }
                else
                {
                    // Otros estados: asignar y resolver nombre de estado en línea
                    pedido.IDEstadoDePedido = datos.IDNuevoEstado;
                    pedido.EstadoActual = await estados
                        .Where(e => e.IDEstadoDePedido == datos.IDNuevoEstado)
                        .Select(e => e.NombreEstado)
                        .FirstOrDefaultAsync() ?? pedido.EstadoActual;
                }

                // Actualizar responsable del cambio
                pedido.IDUsuario = datos.IDUsuario;

                // Registrar historial del nuevo estado
                pedido.HistorialDeEstados ??= new List<HistorialDeEstados>();
                pedido.HistorialDeEstados.Add(new HistorialDeEstados
                {
                    IDPedido = pedido.IDPedido,
                    IDEstadoDePedido = pedido.IDEstadoDePedido,
                    fecha_hora_inicio = DateTime.Now,
                    IDUsuario = datos.IDUsuario,
                    Observaciones = datos.Observaciones
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // Si algo falla, devuelve false para que el controlador responda 400 con mensaje de negocio
                return false;
            }
        }
    }
}