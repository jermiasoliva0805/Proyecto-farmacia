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

            return await query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new OrderSummaryDTO
                {
                    IDPedido = p.IDPedido,
                    Fecha = p.Fecha,
                    Total = p.Total,
                    IDEstadoDePedido = p.IDEstadoDePedido,
                    EstadoNombre = p.EstadoDePedido != null ? p.EstadoDePedido.NombreEstado : "Sin Estado",
                    ClienteNombre = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Sin Cliente"
                })
                .ToListAsync();
        }

        public async Task<Pedido?> GetByIdAsync(int idPedido)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Barrio)
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Localidad)
                .Include(p => p.Sucursal)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IDPedido == idPedido);
        }

        public async Task UpdateAsync(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarEstadoPedidoAsync(ChangeOrderStatusDTO dto)
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IDPedido == dto.IDPedido);
            if (pedido == null) return false;

            if (dto.IDNuevoEstado == 8) // Entrega Fallida
            {
                pedido.IntentosEntregaFallida++;
                if (pedido.IntentosEntregaFallida >= 3)
                {
                    pedido.IDEstadoDePedido = 9; // Cancelado
                    pedido.EstadoActual = "Cancelado";
                }
                else
                {
                    pedido.IDEstadoDePedido = 8;
                    pedido.EstadoActual = "Entrega fallida";
                }
            }
            else
            {
                pedido.IDEstadoDePedido = dto.IDNuevoEstado;
                if (dto.IDNuevoEstado == 7) pedido.IntentosEntregaFallida = 0;
            }

            // Usamos HistorialesDeEstados (Plural) que es como está en tu DB
            var nuevoHistorial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = pedido.IDEstadoDePedido,
                IDUsuario = dto.IDUsuario,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = dto.Observaciones
            };

            _context.HistorialesDeEstados.Add(nuevoHistorial); 
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReporteOperarioDTO>> GetTiempoPromedioArmadoAsync()
        {
            // Buscamos en la tabla de historiales los estados de inicio (2) y fin (4)
            var historiales = await _context.HistorialesDeEstados
                .Include(h => h.Usuario)
                .Where(h => h.IDEstadoDePedido == 2 || h.IDEstadoDePedido == 4)
                .ToListAsync();

            var tiemposPorPedido = historiales
                .GroupBy(h => h.IDPedido)
                .Select(grupo => {
                    var inicio = grupo.FirstOrDefault(h => h.IDEstadoDePedido == 2);
                    var fin = grupo.FirstOrDefault(h => h.IDEstadoDePedido == 4);

                    if (inicio != null && fin != null)
                    {
                        return new {
                            Nombre = inicio.Usuario != null ? $"{inicio.Usuario.Nombre} {inicio.Usuario.Apellido}" : "Operario Desconocido",
                            Minutos = (fin.fecha_hora_inicio - inicio.fecha_hora_inicio).TotalMinutes
                        };
                    }
                    return null;
                })
                .Where(x => x != null)
                .ToList();

            return tiemposPorPedido
                .GroupBy(x => x!.Nombre)
                .Select(g => new ReporteOperarioDTO
                {
                    NombreOperario = g.Key,
                    TotalPedidosArmados = g.Count(),
                    TiempoPromedioMinutos = Math.Round(g.Average(x => x!.Minutos), 2)
                })
                .ToList();
        }
    }
}