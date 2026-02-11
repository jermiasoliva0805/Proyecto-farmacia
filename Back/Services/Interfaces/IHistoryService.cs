using Back.DTOs;

namespace Back.Services.Interfaces
{
    public interface IHistoryService
    {
        Task RegistrarCambioEstado(OrderHistoryDTO dto);
        Task<IEnumerable<OrderHistoryDTO>> ObtenerHistorialPorPedido(int idPedido);
    }
}