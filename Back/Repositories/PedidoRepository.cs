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
                query = query.Where(p => p.IDEstadoDePedido == filters.IDEstadoDePedido.Value);

            if (filters.IDUsuario.HasValue && filters.IDUsuario.Value > 0)
                query = query.Where(p => p.IDUsuario == filters.IDUsuario.Value);

            if (filters.IDCliente.HasValue && filters.IDCliente.Value > 0)
                query = query.Where(p => p.IDCliente == filters.IDCliente.Value);

            if (filters.FechaDesde.HasValue)
                query = query.Where(p => p.Fecha.Date >= filters.FechaDesde.Value.Date);

            if (filters.FechaHasta.HasValue)
                query = query.Where(p => p.Fecha.Date <= filters.FechaHasta.Value.Date);

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
                    FechaEntregaReal = p.FechaEntregaReal,
                    IntentosEntregaFallida = p.IntentosEntregaFallida
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

                var estados = _context.Set<EstadoDePedido>();

                var idEntregado = await estados.Where(e => e.NombreEstado == "Entregado")
                                               .Select(e => e.IDEstadoDePedido)
                                               .FirstOrDefaultAsync();

                var idEntregaFallida = await estados.Where(e => e.NombreEstado == "Entrega fallida")
                                                    .Select(e => e.IDEstadoDePedido)
                                                    .FirstOrDefaultAsync();

                var idCancelado = await estados.Where(e => e.NombreEstado == "Cancelado")
                                               .Select(e => e.IDEstadoDePedido)
                                               .FirstOrDefaultAsync();

                // ✅ Solo bloquear si ya está cancelado
                if (pedido.IDEstadoDePedido == idCancelado)
                    return false;

                if (datos.IDNuevoEstado == idEntregaFallida)
                {
                    pedido.IntentosEntregaFallida++;

                    if (pedido.IntentosEntregaFallida >= MaxIntentosFallidos)
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
                        datos.Observaciones ??= $"Intento de entrega fallido #{pedido.IntentosEntregaFallida}.";
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
                    pedido.IDEstadoDePedido = datos.IDNuevoEstado;
                    pedido.EstadoActual = await estados
                        .Where(e => e.IDEstadoDePedido == datos.IDNuevoEstado)
                        .Select(e => e.NombreEstado)
                        .FirstOrDefaultAsync() ?? pedido.EstadoActual;
                }

                pedido.IDUsuario = datos.IDUsuario;

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
                return false;
            }
        }
    }
}
