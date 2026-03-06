using Back.DTOs;
using Back.Models;

namespace Back.Services.Interfaces
{
    public interface ICancellationService
    {
        /// <summary>
        /// Cancela un pedido con validaciones específicas por motivo
        /// </summary>
        Task<(bool Success, string Message)> CancelarPedidoAsync(CancelarPedidoDTO dto, string usuarioId);

        /// <summary>
        /// Obtiene todos los motivos de cancelación disponibles
        /// </summary>
        Task<List<MotivoCancelacion>> ObtenerMotivosCancelacionAsync();

        /// <summary>
        /// Valida si un pedido puede ser cancelado
        /// </summary>
        Task<(bool CanCancel, string Reason)> ValidarCancelacionAsync(int idPedido);

        // Métodos específicos por motivo de cancelación
        Task<(bool Success, string Message)> CancelarPorArrepentimientoAsync(int idPedido, string usuarioId, string justificacion);
        Task<(bool Success, string Message)> CancelarPorFaltaDeStockAsync(int idPedido, string usuarioId, string justificacion);
        Task<(bool Success, string Message)> CancelarPorErrorPagoAsync(int idPedido, string usuarioId, string justificacion);
        Task<(bool Success, string Message)> CancelarPorDireccionIncorrectaAsync(int idPedido, string usuarioId, string justificacion);
    }
}
