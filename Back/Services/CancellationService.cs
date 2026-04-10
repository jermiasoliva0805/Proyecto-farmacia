using Back.DTOs;
using Back.Interfaces;
using Back.Models;
using Back.Services.Interfaces;
using Back.Repositories.Interfaces;
using System.Threading.Tasks;

namespace Back.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly ICancellationRepository _cancellationRepo;
        private readonly IHistoryRepository _historyRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderStatusRepository _orderStatusRepo;

        public CancellationService(
            ICancellationRepository cancellationRepo,
            IHistoryRepository historyRepo,
            IOrderRepository orderRepo,
            IOrderStatusRepository orderStatusRepo)
        {
            _cancellationRepo = cancellationRepo;
            _historyRepo = historyRepo;
            _orderRepo = orderRepo;
            _orderStatusRepo = orderStatusRepo;
        }

        /// <summary>
        /// Cancela un pedido validando el motivo y aplicando lógica específica
        /// </summary>
        public async Task<(bool Success, string Message)> CancelarPedidoAsync(CancelarPedidoDTO dto, string usuarioId)
        {
            // Validaciones básicas
            if (dto == null || dto.PedidoId <= 0)
                return (false, "Datos de cancelación inválidos.");

            if (dto.MotivoCancelacionId <= 0)
                return (false, "Debe seleccionar un motivo de cancelación.");

            // Validar que el motivo exista y esté activo
            var motivo = await _cancellationRepo.GetMotivoCancelacionAsync(dto.MotivoCancelacionId);
            if (motivo == null)
                return (false, "El motivo de cancelación seleccionado no existe o está inactivo.");

            // Validar que el pedido pueda ser cancelado
            var canCancel = await _cancellationRepo.CanCancelOrderAsync(dto.PedidoId);
            if (!canCancel)
                return (false, "Este pedido no puede ser cancelado en su estado actual.");

            // Delegar a método específico según el motivo
            return motivo.Nombre switch
            {
                "Arrepentimiento" => await CancelarPorArrepentimientoAsync(
                    dto.PedidoId, usuarioId, dto.Justificacion ?? ""),
                "Falta de stock" => await CancelarPorFaltaDeStockAsync(
                    dto.PedidoId, usuarioId, dto.Justificacion ?? ""),
                "Error en el pago" => await CancelarPorErrorPagoAsync(
                    dto.PedidoId, usuarioId, dto.Justificacion ?? ""),
                "Dirección incorrecta" => await CancelarPorDireccionIncorrectaAsync(
                    dto.PedidoId, usuarioId, dto.Justificacion ?? ""),
                _ => await CancelarPorMotivoGenericoAsync(dto.PedidoId, usuarioId, motivo.Nombre, dto.Justificacion ?? "")
            };
        }

        /// <summary>
        /// Obtiene todos los motivos de cancelación disponibles
        /// </summary>
        public async Task<List<MotivoCancelacion>> ObtenerMotivosCancelacionAsync()
        {
            return await _cancellationRepo.GetMotivosCancelacionActivosAsync();
        }

        /// <summary>
        /// Valida si un pedido puede ser cancelado
        /// </summary>
        public async Task<(bool CanCancel, string Reason)> ValidarCancelacionAsync(int idPedido)
        {
            var pedido = await _orderRepo.GetByIdAsync(idPedido);
            if (pedido == null)
                return (false, "El pedido no existe.");

            var canCancel = await _cancellationRepo.CanCancelOrderAsync(idPedido);
            if (!canCancel)
                return (false, $"No se puede cancelar un pedido en estado: {pedido.EstadoActual}");

            return (true, "El pedido puede ser cancelado.");
        }

        /// <summary>
        /// Cancelación por arrepentimiento: Solo se permite si aún no ha sido preparado
        /// </summary>
        public async Task<(bool Success, string Message)> CancelarPorArrepentimientoAsync(
            int idPedido, string usuarioId, string justificacion)
        {
            var pedido = await _orderRepo.GetByIdWithClienteAsync(idPedido);
            if (pedido == null)
                return (false, "Pedido no encontrado.");

            // Validación: Arrepentimiento solo en estados iniciales (Sin preparar - Estado 1)
            if (pedido.IDEstadoDePedido > 1)
                return (false, "No se puede cancelar por arrepentimiento si el pedido ya está siendo preparado.");

            return await EjecutarCancelacionAsync(
                pedido,
                1, // ID motivo arrepentimiento
                "Cancelación por arrepentimiento del cliente.",
                justificacion,
                usuarioId);
        }

        /// <summary>
        /// Cancelación por falta de stock: Se documenta qué productos faltaron
        /// </summary>
        public async Task<(bool Success, string Message)> CancelarPorFaltaDeStockAsync(
            int idPedido, string usuarioId, string justificacion)
        {
            var pedido = await _orderRepo.GetByIdWithClienteAsync(idPedido);
            if (pedido == null)
                return (false, "Pedido no encontrado.");

            // Validación: Se puede cancelar por falta de stock en casi cualquier estado previo a entrega
            if (pedido.IDEstadoDePedido == 6 || pedido.IDEstadoDePedido == 7) // En camino o Entregado
                return (false, "No se puede cancelar por falta de stock si el pedido ya está en camino o fue entregado.");

            var observacionDetallada = $"Cancelación por falta de stock. Productos no disponibles: {justificacion}";

            return await EjecutarCancelacionAsync(
                pedido,
                2, // ID motivo falta de stock
                observacionDetallada,
                justificacion,
                usuarioId);
        }

        /// <summary>
        /// Cancelación por error en pago: Se requiere justificación del error
        /// </summary>
        public async Task<(bool Success, string Message)> CancelarPorErrorPagoAsync(
            int idPedido, string usuarioId, string justificacion)
        {
            var pedido = await _orderRepo.GetByIdWithClienteAsync(idPedido);
            if (pedido == null)
                return (false, "Pedido no encontrado.");

            if (string.IsNullOrWhiteSpace(justificacion))
                return (false, "Debe especificar el tipo de error en el pago (rechazo, fondos insuficientes, etc.).");

            var observacionDetallada = $"Cancelación por error en pago. Tipo de error: {justificacion}";

            return await EjecutarCancelacionAsync(
                pedido,
                3, // ID motivo error pago
                observacionDetallada,
                justificacion,
                usuarioId);
        }

        /// <summary>
        /// Cancelación por dirección incorrecta: Se documenta la dirección problemática
        /// </summary>
        public async Task<(bool Success, string Message)> CancelarPorDireccionIncorrectaAsync(
            int idPedido, string usuarioId, string justificacion)
        {
            var pedido = await _orderRepo.GetByIdWithClienteAsync(idPedido);
            if (pedido == null)
                return (false, "Pedido no encontrado.");

            // Validación: Se puede cancelar por dirección desde estados iniciales hasta en camino
            if (pedido.IDEstadoDePedido == 7) // Entregado
                return (false, "No se puede cancelar por dirección incorrecta si el pedido ya fue entregado.");

            var observacionDetallada = $"Cancelación por dirección incorrecta. Detalles: {justificacion}. Dirección registrada: {pedido.DireccionEntrega}";

            return await EjecutarCancelacionAsync(
                pedido,
                4, // ID motivo dirección incorrecta
                observacionDetallada,
                justificacion,
                usuarioId);
        }

        /// <summary>
        /// Cancelación por otro motivo genérico
        /// </summary>
        private async Task<(bool Success, string Message)> CancelarPorMotivoGenericoAsync(
            int idPedido, string usuarioId, string nombreMotivo, string justificacion)
        {
            var pedido = await _orderRepo.GetByIdWithClienteAsync(idPedido);
            if (pedido == null)
                return (false, "Pedido no encontrado.");

            var observacionDetallada = $"Cancelación por: {nombreMotivo}. Detalles: {justificacion}";

            return await EjecutarCancelacionAsync(
                pedido,
                0, // Sin motivo específico
                observacionDetallada,
                justificacion,
                usuarioId);
        }

        /// <summary>
        /// Ejecuta la cancelación del pedido en la base de datos y registra el historial
        /// </summary>
        private async Task<(bool Success, string Message)> EjecutarCancelacionAsync(
            Pedido pedido,
            int idMotivoCancelacion,
            string observacionHistorial,
            string justificacionPedido,
            string usuarioId)
        {
            try
            {
                // Actualizar el pedido directamente en el repositorio (evita conflictos de tracking)
                await _cancellationRepo.CancelOrderAsync(
                    pedido.IDPedido,
                    idMotivoCancelacion > 0 ? idMotivoCancelacion : null,
                    justificacionPedido
                );

                // Registrar en historial
                var historial = new HistorialDeEstados
                {
                    IDPedido = pedido.IDPedido,
                    IDEstadoDePedido = 9,
                    IDUsuario = int.Parse(usuarioId),
                    fecha_hora_inicio = DateTime.UtcNow,
                    Observaciones = observacionHistorial
                };

                await _historyRepo.AddAsync(historial);

                return (true, $"Pedido #{pedido.IDPedido} cancelado exitosamente.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error en EjecutarCancelacionAsync: {ex}");
                return (false, $"Error al cancelar el pedido: {ex.Message}");
            }
        }
    }
}

