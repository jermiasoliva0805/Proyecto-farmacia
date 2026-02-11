using Back.Models;
using Back.Repositories.Interfaces;
using Back.Services.Interfaces;

namespace Back.Services
{
    public class ClientService : IClientService
    {
        private readonly IGenericRepository<Cliente> _clientRepository;

        public ClientService(IGenericRepository<Cliente> clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<IEnumerable<Cliente>> GetAllClientsAsync()
        {
            return await _clientRepository.GetAllAsync();
        }

        //  para obtener los detalles de un cliente específico
        public async Task<Cliente> GetClientByIdAsync(int id)
        {
            return await _clientRepository.GetByIdAsync(id);
        }
    }
}