using Back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Interfaces
{
    public interface IDeliveryRepository
    {
        Task AddAttemptAsync(IntentoDeEntrega intento); 
        Task<IEnumerable<IntentoDeEntrega>> GetByPedidoIdAsync(int idPedido);
    }
}