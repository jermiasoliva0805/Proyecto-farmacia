using Back.Data;
using Back.Models;
using Back.Repositories;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class LocalityRepository : GenericRepository<Localidad>, ILocalityRepository
{
    public LocalityRepository(AppDbContext context) : base(context) { }
    // RF: Permite obtener las localidades con sus barrios cargados (Eager Loading)
    // Esto es vital para el proceso de registro de clientes y asignación de cadetes
    public async Task<IEnumerable<Localidad>> GetLocalitiesWithBarriosAsync()
    {
        return await _context.Localidades
            .Include(l => l.Barrios)
            .ToListAsync();
    }

    // Obtener una localidad específica detallando sus barrios
    public async Task<Localidad> GetByIdWithBarriosAsync(int id)
    {
        return await _context.Localidades
            .Include(l => l.Barrios)
            .FirstOrDefaultAsync(l => l.IDLocalidad == id);
    }
}