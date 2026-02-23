using Back.Data;
using Back.DTOs;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly AppDbContext _context;

        public ReporteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(
            DateTime fechaDesde, 
            DateTime fechaHasta,
            int? idSucursal = null)
        {
            var inicio = fechaDesde.Date;
            var fin = fechaHasta.Date.AddDays(1).AddTicks(-1);

            Console.WriteLine($"[REPO DEBUG] Rango: {inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd}");

            // Obtener TODOS los pedidos en el rango (sin filtro de cadete primero)
            var todosLosPedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Where(p => p.Fecha >= inicio && p.Fecha <= fin)
                .ToListAsync();

            Console.WriteLine($"[REPO DEBUG] Total pedidos en rango: {todosLosPedidos.Count}");

            // Ver qué roles hay
            var rolesUnicos = todosLosPedidos
                .Where(p => p.Usuario != null)
                .Select(p => p.Usuario.Rol)
                .Distinct()
                .ToList();

            Console.WriteLine($"[REPO DEBUG] Roles únicos encontrados: {string.Join(", ", rolesUnicos)}");

            // Filtrar por sucursal si viene
            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                todosLosPedidos = todosLosPedidos
                    .Where(p => p.IDSucursal == idSucursal.Value)
                    .ToList();

                Console.WriteLine($"[REPO DEBUG] Pedidos después filtro sucursal: {todosLosPedidos.Count}");
            }

            // Filtrar solo los que tienen usuario con rol Cadete
            var pedidosCadete = todosLosPedidos
                .Where(p => p.Usuario != null && 
                            p.Usuario.Rol != null && 
                            p.Usuario.Rol.Trim().ToLower() == "cadete")  // ← Comparación case-insensitive
                .ToList();

            Console.WriteLine($"[REPO DEBUG] Pedidos de cadetes: {pedidosCadete.Count}");

            if (pedidosCadete.Count == 0)
            {
                Console.WriteLine("[REPO DEBUG] ⚠️ No hay pedidos de cadetes!");
                return new List<EntregaPorCadeteDTO>();
            }

            // Agrupar y calcular
            var reporte = pedidosCadete
                .GroupBy(p => new { p.IDUsuario, p.Usuario.Nombre, p.Usuario.Apellido })
                .Select(g =>
                {
                    var pedidosGrupo = g.ToList();
                    var totalAsignados = pedidosGrupo.Count;
                    var entregados = pedidosGrupo.Count(p => p.IDEstadoDePedido == 7);
                    var fallidos = pedidosGrupo.Count(p => p.IDEstadoDePedido == 8 || p.IDEstadoDePedido == 9);
                    var recaudado = pedidosGrupo
                        .Where(p => p.IDEstadoDePedido == 7)
                        .Sum(p => p.Total);

                    var porcentaje = totalAsignados > 0 
                        ? (entregados * 100.0 / totalAsignados) 
                        : 0;

                    Console.WriteLine($"[REPO DEBUG] Cadete: {g.Key.Nombre} {g.Key.Apellido} - Pedidos: {totalAsignados}, Entregados: {entregados}");

                    return new EntregaPorCadeteDTO
                    {
                        IDCadete = g.Key.IDUsuario,
                        NombreCadete = $"{g.Key.Nombre} {g.Key.Apellido}",
                        TotalPedidosAsignados = totalAsignados,
                        EntregasExitosas = entregados,
                        EntregasFallidas = fallidos,
                        TotalRecaudado = recaudado,
                        PorcentajeEfectividad = porcentaje
                    };
                })
                .OrderBy(c => c.NombreCadete)
                .ToList();

            Console.WriteLine($"[REPO DEBUG] Reporte final: {reporte.Count} cadetes");
            return reporte;
        }
        public async Task<List<RankingClienteDTO>> GetRankingClientesFrecuentesAsync()
        {
        return await _context.Pedidos
        .Where(p => p.IDEstadoDePedido == 7) // RN: Solo pedidos con estado 'Entregado'
        .Include(p => p.Cliente)
        .GroupBy(p => new { p.IDCliente, p.Cliente.Nombre })
        .Select(g => new RankingClienteDTO
        {
            NombreCliente = g.Key.Nombre,
            CantidadPedidos = g.Count(),
            GastoTotal = g.Sum(p => p.Total),
            TicketPromedio = g.Count() > 0 ? g.Sum(p => p.Total) / g.Count() : 0,
            UltimaCompra = g.Max(p => p.Fecha)
        })
        .OrderByDescending(x => x.CantidadPedidos) // Orden descendente por volumen
        .Take(10) // Top 10 según Metadata
        .ToListAsync();
}

        
    }
    
}