using Back.Data;
using Back.Models;
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
          return await _context.Pedidos
        .Where(p => p.Fecha >= fechaDesde && p.Fecha <= fechaHasta)
        .Include(p => p.Usuario)
        .GroupBy(p => new 
        { 
            p.IDUsuario, 
            // Concatenamos Nombre y Apellido ya que NombreCompleto no existe en el modelo
            NombreCompleto = p.Usuario.Nombre + " " + p.Usuario.Apellido 
        })
        .Select(g => new EntregaPorCadeteDTO
        {
            IDCadete = g.Key.IDUsuario,
            NombreCadete = g.Key.NombreCompleto,
            TotalPedidosAsignados = g.Count(),
            // Estado 7 = Entregado, Estado 9 = Cancelado
            EntregasExitosas = g.Count(p => p.IDEstadoDePedido == 7),
            EntregasFallidas = g.Count(p => p.IDEstadoDePedido == 9),
            TotalRecaudado = g.Where(p => p.IDEstadoDePedido == 7).Sum(p => p.Total)
        })

        .ToListAsync();
       }
        public async Task<List<RankingClienteDTO>> GetRankingClientesFrecuentesAsync() //clientes frecuentes por volumen jere.
        {
            return await _context.Pedidos
                .Where(p => p.IDEstadoDePedido == 7) // Solo 'Entregado' según Regla de Negocio
                .Include(p => p.Cliente)
                .GroupBy(p => new { p.IDCliente, p.Cliente.Nombre })
                .Select(g => new RankingClienteDTO
                {
                    NombreCliente = g.Key.Nombre,
                    CantidadPedidos = g.Count(),
                    GastoTotal = g.Sum(p => p.Total),
                    // Calculamos promedio: Total / Cantidad
                    TicketPromedio = g.Count() > 0 ? g.Sum(p => p.Total) / g.Count() : 0,
                    // Buscamos la fecha del pedido más reciente
                    UltimaCompra = g.Max(p => p.Fecha)
                })
                .OrderByDescending(x => x.CantidadPedidos) // Orden descendente por volumen
                .Take(10) // Top 10 según Metadata
                .ToListAsync();
        }
    }
}