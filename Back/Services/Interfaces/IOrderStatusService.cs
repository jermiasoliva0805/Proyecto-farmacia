using Back.DTOs;

namespace Back.Services.Interfaces
{
    public interface IOrderStatusService
    {
        // Métodos de asignación para el Admin
        Task<bool> AsignarOperarioAsync(AssignOperatorDTO dto);
        Task<bool> AsignarCadeteAsync(AssignDeliveryDTO dto);

        // Método general de cambio de estado (Cadete/Admin)
        Task<bool> CambiarEstadoAsync(ChangeOrderStatusDTO changeStatusDto);

        // CORREGIDO: Ahora acepta el userId para el historial de estados
        Task<bool> CancelarPedidoAsync(CancelarPedidoDTO dto);
        //Task<bool> CancelarPedidoAsync(CancelarPedidoDTO dto, int userId);
    }
}