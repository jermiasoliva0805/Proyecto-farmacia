using Back.Models;
using Back.Repositories.Interfaces;
using Back.Services.Interfaces;

namespace Back.Services
{
    public class LocalidadService : ILocalidadService
    {
        private readonly IGenericRepository<Localidad> _localidadRepository;

        public LocalidadService(IGenericRepository<Localidad> localidadRepository)
        {
            _localidadRepository = localidadRepository;
        }

        public async Task<IEnumerable<Localidad>> GetAllLocalidadesAsync()
        {
            // Usamos tu repositorio genérico para traer todas las localidades
            return await _localidadRepository.GetAllAsync();
        }

        // Agregamos la obtención por ID para completar la gestión individual
        public async Task<Localidad> GetLocalidadByIdAsync(int id)
        {
            return await _localidadRepository.GetByIdAsync(id);
        }
    }

}
