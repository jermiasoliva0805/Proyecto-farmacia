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
            // Ajuste de rango de fechas (Lógica de tu compañera para incluir todo el último día)
            var inicio = fechaDesde.Date;
            var fin = fechaHasta.Date.AddDays(1).AddTicks(-1);

            // Consulta base con Include para obtener datos del Usuario
            var pedidosQuery = _context.Pedidos
                .Where(p => p.Fecha >= inicio && p.Fecha <= fin)
                .Include(p => p.Usuario)
                .AsNoTracking()
                .AsQueryable();

            // Filtro por sucursal (Mantenemos la funcionalidad extra de tu compa)
            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                pedidosQuery = pedidosQuery.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var pedidosPeriodo = await pedidosQuery.ToListAsync();

            // Agrupación y cálculo de métricas (Combinando ambas lógicas)
            var reportePorCadete = pedidosPeriodo
                .Where(p => p.Usuario != null && 
                    p.Usuario.Rol != null && 
                       p.Usuario.Rol.Trim() == "Cadete") // Aseguramos que solo sean cadetes
                .GroupBy(p => new 
                { 
                    p.IDUsuario, 
                    p.Usuario.Nombre, 
                    p.Usuario.Apellido 
                })
                .Select(g =>
                {
                    var pedidosGrupo = g.ToList();

                    return new EntregaPorCadeteDTO
                    {
                        IDCadete = g.Key.IDUsuario,
                        // Concatenación de nombre y apellido como en tu lógica
                        NombreCadete = $"{g.Key.Nombre} {g.Key.Apellido}",
                        TotalPedidosAsignados = pedidosGrupo.Count,
                        // Estado 7 = Entregado
                        EntregasExitosas = pedidosGrupo.Count(p => p.IDEstadoDePedido == 7),
                        // Estado 9 = Cancelado (Lógica compartida)
                        EntregasFallidas = pedidosGrupo.Count(p => p.IDEstadoDePedido == 9),
                        // Suma de totales solo de entregas exitosas
                        TotalRecaudado = pedidosGrupo
                            .Where(p => p.IDEstadoDePedido == 7)
                            .Sum(p => p.Total)
                    };
                })
                .Where(c => c.TotalPedidosAsignados > 0)
                .OrderBy(c => c.NombreCadete)
                .ToList();

            return reportePorCadete;
        }
    }
}