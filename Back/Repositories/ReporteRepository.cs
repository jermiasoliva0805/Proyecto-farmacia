using Back.Data;
using Back.Models;
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

        public async Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _context.Pedidos
                .Where(p => p.Fecha >= fechaDesde && p.Fecha <= fechaHasta)
                .Include(p => p.Usuario)
                .GroupBy(p => new 
                { 
                    p.IDUsuario, 
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
    }
}