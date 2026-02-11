using Back.DTOs;
using Back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz que define las operaciones de datos para los Pedidos (RF13, RF14, RF20).
    /// </summary>
    public interface IPedidoRepository
    {
        Task<IEnumerable<OrderSummaryDTO>> GetFilteredOrdersAsync(OrderFilterDTO filters);

        // Obtenemos el objeto completo para validaciones complejas en el Service
        Task<Pedido> GetByIdAsync(int id);

        // Este método ahora debe ser capaz de procesar la cancelación (Motivo)
        Task<bool> ActualizarEstadoPedidoAsync(ChangeOrderStatusDTO datos);

        // Método genérico para guardar cambios en el objeto Pedido
        Task<bool> UpdateAsync(Pedido pedido);
    }
}