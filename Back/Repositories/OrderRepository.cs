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

        public async Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            var reporte = await _context.Usuarios
                .Where(u => u.Rol == "Cadete")
                .Select(cadete => new EntregaPorCadeteDTO
                {
                    IDCadete = cadete.IDUsuario,
                    NombreCadete = cadete.Nombre + " " + cadete.Apellido,

                    TotalPedidosAsignados = _context.Pedidos
                        .Count(p => p.IDUsuario == cadete.IDUsuario &&
                                    p.Fecha >= fechaDesde && p.Fecha <= fechaHasta),

                    EntregasExitosas = _context.Pedidos
                        .Count(p => p.IDUsuario == cadete.IDUsuario &&
                                    p.IDEstadoDePedido == 7 &&
                                    p.Fecha >= fechaDesde && p.Fecha <= fechaHasta),

                    EntregasFallidas = _context.Pedidos
                        .Count(p => p.IDUsuario == cadete.IDUsuario &&
                                    p.IDEstadoDePedido == 8 &&
                                    p.Fecha >= fechaDesde && p.Fecha <= fechaHasta),

                    TotalRecaudado = _context.Pedidos
                        .Where(p => p.IDUsuario == cadete.IDUsuario &&
                                    p.IDEstadoDePedido == 7 &&
                                    p.Fecha >= fechaDesde && p.Fecha <= fechaHasta)
                        .Sum(p => (decimal?)p.Total) ?? 0,

                    PorcentajeEfectividad = _context.Pedidos
                        .Count(p => p.IDUsuario == cadete.IDUsuario &&
                                    p.Fecha >= fechaDesde && p.Fecha <= fechaHasta) > 0
                        ? (double)_context.Pedidos.Count(p => p.IDUsuario == cadete.IDUsuario &&
                                                              p.IDEstadoDePedido == 7 &&
                                                              p.Fecha >= fechaDesde && p.Fecha <= fechaHasta)
                          / _context.Pedidos.Count(p => p.IDUsuario == cadete.IDUsuario &&
                                                        p.Fecha >= fechaDesde && p.Fecha <= fechaHasta) * 100
                        : 0
                })
                .ToListAsync();

            return reporte;
        }
    }
}
