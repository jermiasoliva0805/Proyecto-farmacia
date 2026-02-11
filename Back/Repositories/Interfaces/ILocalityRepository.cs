using Back.Models;


namespace Back.Repositories.Interfaces
{
    // Interfaz específica para Localidades
    public interface ILocalityRepository : IGenericRepository<Localidad>
    {
        // RF: Útil para validaciones de zonas de entrega y filtrado
        Task<IEnumerable<Localidad>> GetLocalitiesWithBarriosAsync();
        Task<Localidad> GetByIdWithBarriosAsync(int id);
    }
}