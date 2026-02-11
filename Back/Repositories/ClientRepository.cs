using Back.Data;
using Back.Models;
using Back.Repositories;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ClientRepository : GenericRepository<Cliente>, IClientRepository
{
    public ClientRepository(AppDbContext context) : base(context) { }
    

    public async Task<IEnumerable<Cliente>> SearchClientsAsync(string term)
    {
        return await _context.Clientes
            .Where(c => c.Nombre.Contains(term) || c.DNI.Contains(term))
            .ToListAsync();
    }

    public async Task<Cliente> GetByCuitAsync(string cuit)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.DNI == cuit);
    }
}

