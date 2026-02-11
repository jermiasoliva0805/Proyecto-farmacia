using Back.Models;
using Back.Interfaces;
using Back.Data;
using Microsoft.EntityFrameworkCore;

namespace Back.Repositories
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly AppDbContext _context;

        public DeliveryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAttemptAsync(IntentoDeEntrega intento)
        {
            await _context.IntentosDeEntregas.AddAsync(intento); // Nombre exacto del DbSet
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<IntentoDeEntrega>> GetByPedidoIdAsync(int idPedido)
        {
            return await _context.IntentosDeEntregas
                .Where(i => i.IDPedido == idPedido)
                .ToListAsync();
        }
    }
}