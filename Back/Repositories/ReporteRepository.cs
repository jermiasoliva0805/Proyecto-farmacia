using Back.Data;
using Back.DTOs;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

            // Traer todos los pedidos en el rango
            var pedidosQuery = _context.Pedidos
                .Where(p => p.Fecha >= inicio && p.Fecha <= fin)
                .Include(p => p.Usuario)
                .AsNoTracking();

            // Filtrar por sucursal si es necesario
            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                pedidosQuery = pedidosQuery.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var pedidosPeriodo = await pedidosQuery.ToListAsync();

            // Agrupar por cadete y calcular métricas
            var reportePorCadete = pedidosPeriodo
                .Where(p => p.Usuario != null && p.Usuario.Rol != null && p.Usuario.Rol.Trim() == "Cadete")
                .GroupBy(p => new { p.IDUsuario, p.Usuario.Nombre, p.Usuario.Apellido })
                .Select(g => 
                {
                    var pedidosGrupo = g.ToList();

                    var totalAsignados = pedidosGrupo.Count;
                    
                    // ENTREGADOS: Estado = 7
                    var entregados = pedidosGrupo.Count(p => p.IDEstadoDePedido == 7);
                    
                    // FALLIDAS: Estado = 9 (Cancelado) - SOLO ESTADO 9
                    var fallidosTotal = pedidosGrupo.Count(p => p.IDEstadoDePedido == 9);

                    var recaudado = pedidosGrupo
                        .Where(p => p.IDEstadoDePedido == 7)
                        .Sum(p => p.Total);

                    return new EntregaPorCadeteDTO
                    {
                        IDCadete = g.Key.IDUsuario,
                        NombreCadete = $"{g.Key.Nombre} {g.Key.Apellido}",
                        TotalPedidosAsignados = totalAsignados,
                        EntregasExitosas = entregados,
                        EntregasFallidas = fallidosTotal,  // ← SOLO ESTADO 9
                        TotalRecaudado = recaudado
                    };
                })
                .Where(c => c.TotalPedidosAsignados > 0)
                .OrderBy(c => c.NombreCadete)
                .ToList();

            return reportePorCadete;
        }
    }
}