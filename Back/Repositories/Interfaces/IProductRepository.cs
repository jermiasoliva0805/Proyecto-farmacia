using Back.Models;

namespace Back.Repositories.Interfaces
{
    // Interfaz específica para Productos
    public interface IProductRepository : IGenericRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetActiveProductsAsync();
    }
}