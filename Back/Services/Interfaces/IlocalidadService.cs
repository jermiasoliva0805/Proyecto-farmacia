using Back.Models;

namespace Back.Services.Interfaces
{
    public interface ILocalidadService
    {
        // Motivo: Definir el contrato para obtener localidades sin depender de otras interfaces.
        Task<IEnumerable<Localidad>> GetAllLocalidadesAsync();

        // Agregamos la definición para obtener una localidad específica por su ID
        Task<Localidad> GetLocalidadByIdAsync(int id);
    }
}
