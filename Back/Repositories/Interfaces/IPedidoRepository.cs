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
        // Consultas filtradas para reportes y administración
        Task<IEnumerable<OrderSummaryDTO>> GetFilteredOrdersAsync(OrderFilterDTO filters);

        // Obtenemos el objeto completo para validaciones complejas en el Service
        Task<Pedido?> GetByIdAsync(int idPedido);

        // Este método ahora debe ser capaz de procesar la cancelación (Motivo)
        Task<bool> ActualizarEstadoPedidoAsync(ChangeOrderStatusDTO datos);

        // Actualización genérica de un pedido
        Task UpdateAsync(Pedido pedido);

        // AGREGADO PARA EL REPORTE DE HOY (RF6.4)
        // Se agregan parámetros con valores por defecto para no romper la implementación actual
        Task<List<ReporteOperarioDTO>> GetTiempoPromedioArmadoAsync(int dias = 7, int? idSucursal = null);
        
    }
}