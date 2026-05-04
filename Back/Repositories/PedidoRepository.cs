using Back.Data;
using Back.DTOs;
using Back.Hubs;
using Back.Models;
using Back.Repositories.Interfaces;
using Back.Utils;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<PedidosHub> _hubContext;

        public PedidoRepository(AppDbContext context, IHubContext<PedidosHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<OrderSummaryDTO>> GetFilteredOrdersAsync(OrderFilterDTO filters)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente).ThenInclude(c => c.Localidad)
                .Include(p => p.EstadoDePedido)
                .Include(p => p.Usuario)
                .Include(p => p.Zona)
                .AsNoTracking()
                .AsQueryable();

            // Filtro por usuario (cadete/operario)
            if (filters.IDUsuario.HasValue && filters.IDUsuario.Value > 0)
                query = query.Where(p => p.IDUsuario == filters.IDUsuario.Value);

            // Filtro de búsqueda por ID o nombre de cliente
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                string term = filters.Search.ToLower();
                query = query.Where(p =>
                    p.IDPedido.ToString().Contains(term) ||
                    (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(term)) ||
                    (p.Cliente != null && p.Cliente.Apellido.ToLower().Contains(term)));
            }

            // Filtro por estado principal
            if (filters.IDEstadoDePedido.HasValue && filters.IDEstadoDePedido.Value > 0)
                query = query.Where(p => p.IDEstadoDePedido == filters.IDEstadoDePedido.Value);

            // Filtro por subestado demorado (puede combinarse con IDEstadoDePedido)
            if (filters.SoloDemorados == true)
                query = query.Where(p => p.EsDemorado);

            // Filtro por Fecha Desde
            if (filters.FechaDesde.HasValue)
                query = query.Where(p => p.Fecha >= filters.FechaDesde.Value);

            // Filtro por Fecha Hasta (incluye todo el día)
            if (filters.FechaHasta.HasValue)
            {
                var fechaHastaLimite = filters.FechaHasta.Value.AddDays(1);
                query = query.Where(p => p.Fecha < fechaHastaLimite);
            }

            return await query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new OrderSummaryDTO
                {
                    IDPedido               = p.IDPedido,
                    Fecha                  = p.Fecha,
                    Total                  = p.Total,
                    IDEstadoDePedido       = p.IDEstadoDePedido,
                    EstadoNombre           = p.EstadoDePedido != null ? p.EstadoDePedido.NombreEstado : "Sin Estado",
                    ClienteNombre          = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Sin Cliente",
                    ResponsableNombre      = p.Usuario != null ? $"{p.Usuario.Nombre} {p.Usuario.Apellido}" : "Sin asignar",
                    ResponsableRol         = p.Usuario != null ? p.Usuario.Rol : string.Empty,
                    FechaEntregaReal       = p.FechaEntregaReal,
                    IntentosEntregaFallida = p.IntentosEntregaFallida,
                    FechaEntregaEstimada   = p.FechaEntregaEstimada,
                    ZonaNombre             = p.Zona != null ? p.Zona.Nombre : "Sin asignar",
                    DireccionEntrega       = p.DireccionEntrega,
                    LocalidadNombre        = p.Cliente != null && p.Cliente.Localidad != null ? p.Cliente.Localidad.Ciudad : null,
                    CodigoPostalEntrega    = p.CodigoPostalEntrega,
                    EstaDemorado           = p.EsDemorado,
                    FechaMarcadoDemorado   = p.FechaMarcadoDemorado,
                    FechaInicioArmado      = p.FechaInicioArmado.HasValue ? p.FechaInicioArmado.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    FechaFinArmado         = p.FechaFinArmado.HasValue ? p.FechaFinArmado.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
                })
                .ToListAsync();
        }

        private Task MarcarPedidosDemoradosAutomaticamenteAsync() => Task.CompletedTask;

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

            if (dto.IDNuevoEstado == 8) // Entrega fallida
            {
                pedido.IntentosEntregaFallida++;
                if (pedido.IntentosEntregaFallida >= 3)
                {
                    estadoFinal = 9;
                    pedido.IDEstadoDePedido = 9;
                    pedido.EstadoActual = "Cancelado";
                    pedido.Estado = "Cancelado";
                    pedido.JustificacionCancelacion = "Superó los 3 intentos fallidos.";
                    // EsDemorado se mantiene intacto — queda para el reporte histórico
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

                if (dto.IDNuevoEstado == 7) // Entregado
                {
                    pedido.IntentosEntregaFallida = 0;
                    pedido.EstadoActual = "Entregado";
                    pedido.Estado = "Entregado";
                    pedido.FechaEntregaReal = DateTimeHelper.GetArgentinaTime();
                    // EsDemorado se mantiene intacto — queda para el reporte histórico
                    _context.Entry(pedido).State = EntityState.Modified;
                }
                else if (dto.IDNuevoEstado == 9) // Cancelado
                {
                    pedido.EstadoActual = "Cancelado";
                    pedido.Estado = "Cancelado";
                    // EsDemorado se mantiene intacto — queda para el reporte histórico
                }
            }

            var nuevoHistorial = new HistorialDeEstados
            {
                IDPedido          = pedido.IDPedido,
                IDEstadoDePedido  = estadoFinal,
                IDUsuario         = dto.IDUsuario,
                fecha_hora_inicio = DateTime.UtcNow,
                Observaciones     = dto.Observaciones
            };
            _context.HistorialesDeEstados.Add(nuevoHistorial);
            await _context.SaveChangesAsync();

            // Notificar a todos los clientes para que refresquen la campanita
            await _hubContext.Clients.All.SendAsync("PedidosDemoradosActualizados", new List<object>());

            return true;
        }

        public async Task<List<ReporteOperarioDTO>> GetTiempoPromedioArmadoAsync(int dias = 7, int? idSucursal = null)
        {
            DateTime fechaInicioFiltro = DateTime.Now.AddDays(-dias);
            const int UMBRAL_MINUTOS = 30;

            Console.WriteLine($"[REPO DEBUG] GetTiempoPromedioArmadoAsync - Dias: {dias}, IdSucursal: {idSucursal}");
            Console.WriteLine($"[REPO DEBUG] FechaInicio Filtro: {fechaInicioFiltro:yyyy-MM-dd}");

            var query = _context.HistorialesDeEstados
                .Include(h => h.Usuario)
                .Include(h => h.Pedido)
                .Where(h => h.Usuario != null && !h.Usuario.IsDeleted)
                .Where(h => h.fecha_hora_inicio >= fechaInicioFiltro)
                .Where(h => h.IDEstadoDePedido == 2 || h.IDEstadoDePedido == 4)
                .AsQueryable();

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                Console.WriteLine($"[REPO DEBUG] Aplicando filtro de sucursal: {idSucursal.Value}");
                query = query.Where(h => h.Pedido != null && h.Pedido.IDSucursal == idSucursal.Value);
            }

            var historiales = await query.ToListAsync();
            Console.WriteLine($"[REPO DEBUG] Total historiales encontrados: {historiales.Count}");

            var reporte = historiales
                .GroupBy(h => h.IDPedido)
                .Select(g => new
                {
                    PedidoId = g.Key,
                    Inicio   = g.Where(h => h.IDEstadoDePedido == 2).OrderBy(h => h.fecha_hora_inicio).Select(h => h.fecha_hora_inicio).FirstOrDefault(),
                    Fin      = g.Where(h => h.IDEstadoDePedido == 4).OrderBy(h => h.fecha_hora_inicio).Select(h => h.fecha_hora_inicio).FirstOrDefault(),
                    NombreCompleto = g.Where(h => h.Usuario != null)
                        .Select(h => h.Usuario != null ? $"{h.Usuario.Nombre} {h.Usuario.Apellido}" : null)
                        .FirstOrDefault(n => !string.IsNullOrEmpty(n))
                })
                .Where(x => x.Inicio != default && x.Fin != default && !string.IsNullOrEmpty(x.NombreCompleto))
                .Select(x => new { x.NombreCompleto, Minutos = (x.Fin - x.Inicio).TotalMinutes })
                .GroupBy(x => x.NombreCompleto)
                .Select(g =>
                {
                    int totales = g.Count();
                    int dentro  = g.Count(x => x.Minutos <= UMBRAL_MINUTOS);
                    return new ReporteOperarioDTO
                    {
                        NombreOperario        = g.Key ?? string.Empty,
                        PedidosTotales        = totales,
                        DentroUmbral          = dentro,
                        FueraUmbral           = totales - dentro,
                        TiempoPromedioMinutos = Math.Round(g.Average(x => x.Minutos), 2),
                        PorcentajeEficiencia  = Math.Round((double)dentro / totales * 100, 2)
                    };
                })
                .OrderByDescending(r => r.PorcentajeEficiencia)
                .ToList();

            Console.WriteLine($"[REPO DEBUG] Reporte final: {reporte.Count} operarios");
            return reporte;
        }
    }
}