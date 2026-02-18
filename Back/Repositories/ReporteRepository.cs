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
                .GroupJoin(
                    _context.Pedidos.Where(p => p.Fecha >= fechaDesde && p.Fecha <= fechaHasta),
                    cadete => cadete.IDUsuario,   // PK del cadete
                    pedido => pedido.IDUsuario,   // FK en Pedido
                    (cadete, pedidos) => new EntregaPorCadeteDTO
                    {
                        IDCadete = cadete.IDUsuario,
                        NombreCadete = cadete.Nombre + " " + cadete.Apellido,
                        TotalPedidosAsignados = pedidos.Count(),

                        EntregasExitosas = pedidos.Count(p => p.IDEstadoDePedido == 7), // 7 = Entregado
                        EntregasFallidas = pedidos.Count(p => p.IDEstadoDePedido == 8), // 8 = Fallido

                        TotalRecaudado = pedidos.Where(p => p.IDEstadoDePedido == 7)
                                                .Sum(p => (decimal?)p.Total) ?? 0,

                        PorcentajeEfectividad = pedidos.Count() > 0
                            ? (double)pedidos.Count(p => p.IDEstadoDePedido == 7) / pedidos.Count() * 100
                            : 0
                    }
                )
                .ToListAsync();

            return reporte;
        }
    }
}
