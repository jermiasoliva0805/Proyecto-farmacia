using Back.Models;
using Back.Interfaces;
using Back.Data;
using Microsoft.EntityFrameworkCore;

namespace Back.Repositories
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly AppDbContext _context; 

        public HistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(HistorialDeEstados historial)
        {
            await _context.HistorialesDeEstados.AddAsync(historial); 
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<HistorialDeEstados>> GetByPedidoIdAsync(int idPedido)
        {
            return await _context.HistorialesDeEstados
                .Where(h => h.IDPedido == idPedido)
                .OrderByDescending(h => h.fecha_hora_inicio)
                .ToListAsync();
        }
    }
}