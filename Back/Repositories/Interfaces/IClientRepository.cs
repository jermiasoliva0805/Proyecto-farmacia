using Back.Models;

namespace Back.Repositories.Interfaces
{
    // Interfaz específica para Clientes
    public interface IClientRepository : IGenericRepository<Cliente>
    {
        // El RF13 menciona filtrar. Aquí permitimos buscar por nombre o CUIT
        Task<IEnumerable<Cliente>> SearchClientsAsync(string term);

        // Útil para validar que no existan duplicados antes de insertar
        Task<Cliente> GetByCuitAsync(string cuit);
    }
}
