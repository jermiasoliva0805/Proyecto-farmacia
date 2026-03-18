using Back.Models;
using Back.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Pedido>
    {
        Task<int> CreateOrderAsync(Pedido pedido, int idUsuario);
        Task<Pedido> GetOrderWithDetailsAsync(int id);

        // Lista de pedidos por estado (resumidos)
        Task<IEnumerable<OrderSummaryDTO>> GetOrdersByStatusAsync(int statusId);

        // NUEVO: trae el pedido incluyendo el Cliente (para obtener el mail)
        Task<Pedido> GetByIdWithClienteAsync(int id);

        // NUEVO: obtiene todos los pedidos de un cliente con sus detalles
        Task<IEnumerable<Pedido>> GetClientOrdersAsync(int clientId);
    }
}