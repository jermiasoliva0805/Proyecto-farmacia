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

            // Filtro por Fecha Desde
            if (filters.FechaDesde.HasValue)
            {
                // Usamos .Date para comparar solo el día, o >= si queremos desde el inicio del día
                query = query.Where(p => p.Fecha >= filters.FechaDesde.Value);
            }

            // Filtro por Fecha Hasta
            if (filters.FechaHasta.HasValue)
            {
                // Para incluir los pedidos de TODO el día hasta el final, sumamos 1 día
                var fechaHastaLimite = filters.FechaHasta.Value.AddDays(1);
                query = query.Where(p => p.Fecha < fechaHastaLimite);
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
                    ClienteNombre = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Sin Cliente"
                })
                .ToListAsync();
        }

        public async Task<Pedido?> GetByIdAsync(int idPedido)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente).ThenInclude(c => c.Barrio)
                .Include(p => p.Cliente).ThenInclude(c => c.Localidad)
                .Include(p => p.Sucursal)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
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

            int estadoFinal = dto.IDNuevoEstado;

            if (dto.IDNuevoEstado == 8) 
            {
                pedido.IntentosEntregaFallida++;
                if (pedido.IntentosEntregaFallida >= 3)
                {
                    estadoFinal = 9;
                    pedido.IDEstadoDePedido = 9;
                    pedido.EstadoActual = "Cancelado";
                    pedido.JustificacionCancelacion = "Superó los 3 intentos fallidos.";
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

            var nuevoHistorial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = estadoFinal,
                IDUsuario = dto.IDUsuario,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = dto.Observaciones
            };

            _context.HistorialesDeEstados.Add(nuevoHistorial);
            await _context.SaveChangesAsync();
            return true;
        }

        // REPORTE ACTUALIZADO PARA DASHBOARD (RF6.4)
        public async Task<List<ReporteOperarioDTO>> GetTiempoPromedioArmadoAsync(int dias = 7, int? idSucursal = null)
        {
            DateTime fechaInicioFiltro = DateTime.Now.AddDays(-dias);
            const int UMBRAL_MINUTOS = 30; // Definido para el análisis de eficiencia

            var query = _context.HistorialesDeEstados
                .Include(h => h.Usuario)
                .Include(h => h.Pedido)
                .Where(h => h.fecha_hora_inicio >= fechaInicioFiltro)
                .Where(h => h.IDEstadoDePedido == 2 || h.IDEstadoDePedido == 4)
                .AsQueryable();

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(h => h.Pedido.IDSucursal == idSucursal.Value);
            }

            var historiales = await query.ToListAsync();

            var reporte = historiales
                .GroupBy(h => h.IDPedido)
                .Select(g => new
                {
                    PedidoId = g.Key,
                    Inicio = g.Where(h => h.IDEstadoDePedido == 2).OrderBy(h => h.fecha_hora_inicio).Select(h => h.fecha_hora_inicio).FirstOrDefault(),
                    Fin = g.Where(h => h.IDEstadoDePedido == 4).OrderBy(h => h.fecha_hora_inicio).Select(h => h.fecha_hora_inicio).FirstOrDefault(),
                    NombreCompleto = g.Where(h => h.Usuario != null)
                                    .Select(h => $"{h.Usuario.Nombre} {h.Usuario.Apellido}")
                                    .FirstOrDefault()
                })
                .Where(x => x.Inicio != default && x.Fin != default && !string.IsNullOrEmpty(x.NombreCompleto))
                .Select(x => new 
                { 
                    x.NombreCompleto, 
                    Minutos = (x.Fin - x.Inicio).TotalMinutes 
                })
                .GroupBy(x => x.NombreCompleto)
                .Select(g => 
                {
                    int totales = g.Count();
                    int dentro = g.Count(x => x.Minutos <= UMBRAL_MINUTOS);
                    return new ReporteOperarioDTO
                    {
                        NombreOperario = g.Key,
                        PedidosTotales = totales, // Antes era TotalPedidosArmados
                        DentroUmbral = dentro,
                        FueraUmbral = totales - dentro,
                        TiempoPromedioMinutos = Math.Round(g.Average(x => x.Minutos), 2),
                        PorcentajeEficiencia = Math.Round((double)dentro / totales * 100, 2)
                    };
                })
                .OrderByDescending(r => r.PorcentajeEficiencia)
                .ToList();

            return reporte;
        }
    }
}