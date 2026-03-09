using Back.Models;

namespace Back.Interfaces
{
    public interface ICancellationRepository
    {
        Task CancelOrderAsync(int idPedido, int? motivoCancelacionId = null, string justificacion = "");

        /// <summary>
        /// Obtiene un motivo de cancelación activo por su ID
        /// </summary>
        Task<MotivoCancelacion?> GetMotivoCancelacionAsync(int idMotivo);

        /// <summary>
        /// Obtiene todos los motivos de cancelación activos
        /// </summary>
        Task<List<MotivoCancelacion>> GetMotivosCancelacionActivosAsync();

        /// <summary>
        /// Valida si un pedido puede ser cancelado
        /// </summary>
        Task<bool> CanCancelOrderAsync(int idPedido);

        /// <summary>
        /// Obtiene información del pedido para validación de cancelación
        /// </summary>
        Task<(Pedido? Pedido, int IntentosEntrega)> GetOrderDetailForCancellationAsync(int idPedido);
    }
}