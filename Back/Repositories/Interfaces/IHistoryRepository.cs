using Back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Interfaces
{
    public interface IHistoryRepository
    {
        Task AddAsync(HistorialDeEstados historial); 
        Task<IEnumerable<HistorialDeEstados>> GetByPedidoIdAsync(int idPedido);
    }
}