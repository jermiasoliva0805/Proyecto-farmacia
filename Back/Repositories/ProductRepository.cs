using Back.Data;
using Back.Models;
using Back.Repositories;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ProductRepository : GenericRepository<Producto>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }
    public async Task<IEnumerable<Producto>> GetActiveProductsAsync()
    {
        // Filtramos los productos que tengan stock disponible (según lógica de negocio)
        return await _context.Productos
            .Where(p => p.CantidadProducto > 0)
            .ToListAsync();
    }

    // Método extra para buscar productos por nombre (útil para RF13 de filtros)
    public async Task<IEnumerable<Producto>> SearchProductsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetActiveProductsAsync();

        return await _context.Productos
            .Where(p => p.NombreProducto.Contains(term) || p.Descripcion.Contains(term))
            .ToListAsync();
    }
}

