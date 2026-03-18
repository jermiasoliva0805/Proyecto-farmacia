using AutoMapper;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Back.Services.Interfaces;
using Back.Services; // EmailSender

namespace Back.Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private readonly IOrderStatusRepository _repository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ClientProductRelationService _clientProductRelationService;

        public OrderStatusService(
            IOrderStatusRepository repository,
            IOrderRepository orderRepository,
            IMapper mapper,
            EmailSender emailSender,
            IConfiguration configuration,
            ClientProductRelationService clientProductRelationService)
        {
            _repository = repository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailSender = emailSender;
            _configuration = configuration;
            _clientProductRelationService = clientProductRelationService;
        }

        /// <summary>
        /// Genera la URL de tracking para un pedido
        /// </summary>
        private string GenerarTrackingUrl(int pedidoId)
        {
            var baseUrl = _configuration["AppSettings:FrontendUrl"]
                ?? Environment.GetEnvironmentVariable("FRONTEND_URL")
                ?? "https://midominio.com";

            if (baseUrl.EndsWith("/"))
                baseUrl = baseUrl.TrimEnd('/');

            return $"{baseUrl}/tracking/{pedidoId}";
        }

        /// <summary>
        /// Extrae los nombres de los productos de un pedido para personalizar emails
        /// </summary>
        private List<string> ObtenerNombresProductos(Pedido pedido)
        {
            var nombres = new List<string>();
            if (pedido?.Detalles != null && pedido.Detalles.Any())
            {
                foreach (var detalle in pedido.Detalles)
                {
                    if (detalle.Producto != null && !string.IsNullOrEmpty(detalle.Producto.NombreProducto))
                    {
                        nombres.Add(detalle.Producto.NombreProducto);
                    }
                }
                nombres = nombres.Distinct().Take(3).ToList();
            }
            return nombres;
        }

        // 1. ASIGNAR OPERARIO (ADMIN) - Pasa de Sin preparar (1) a Preparar pedido (2)
        public async Task<bool> AsignarOperarioAsync(AssignOperatorDTO dto)
        {
            // Trae Cliente y Detalles para poder enviar correo personalizado
            var pedido = await _orderRepository.GetByIdWithClienteAndDetailsAsync(dto.PedidoId);

            if (pedido == null || pedido.IDEstadoDePedido != 1) return false;

            pedido.IDEstadoDePedido = 2; // Preparar pedido
            pedido.IDUsuario = dto.OperarioId;

            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 2,
                IDUsuario = dto.OperarioId,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = "Admin asignó operario para la preparación del pedido."
            };

            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre  // Solo el nombre, como en "sin preparar"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(2);

                // Obtener etiqueta logística
                var etiquetaLogistica = await _clientProductRelationService.ObtenerEtiquetaLogisticaAsync(
                    pedido.IDCliente, 
                    pedido.IDPedido);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        var trackingUrl = GenerarTrackingUrl(pedido.IDPedido);
                        var nombresProductos = ObtenerNombresProductos(pedido);
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            2,                                  // Preparar pedido
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP",
                            trackingUrl: trackingUrl,
                            etiquetaLogistica: etiquetaLogistica,
                            nombresProductos: nombresProductos
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enviando mail (Preparar pedido): {ex.Message}");
                    }
                }
            }

            return resultado;
        }

        // 2. ASIGNAR CADETE (ADMIN) - Pasa de Listo para despachar (4) a Despachando (5)
        public async Task<bool> AsignarCadeteAsync(AssignDeliveryDTO dto)
        {
            // Trae Cliente y Detalles para poder enviar correo personalizado
            var pedido = await _orderRepository.GetByIdWithClienteAndDetailsAsync(dto.PedidoId);

            // El pedido debe estar en 4 (Listo para despachar) para pasar a 5 (Despachando)
            if (pedido == null || pedido.IDEstadoDePedido != 4) return false;

            pedido.IDEstadoDePedido = 5; // Despachando
            pedido.IDUsuario = dto.CadeteId;

            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 5,
                IDUsuario = dto.CadeteId,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = "Cadete asignado. Pedido en despachando."
            };

            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre  // Solo el nombre, como en "sin preparar"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(5);

                // Obtener etiqueta logística
                var etiquetaLogistica = await _clientProductRelationService.ObtenerEtiquetaLogisticaAsync(
                    pedido.IDCliente, 
                    pedido.IDPedido);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        var trackingUrl = GenerarTrackingUrl(pedido.IDPedido);
                        var nombresProductos = ObtenerNombresProductos(pedido);
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            5,                                  // Despachando
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP",
                            trackingUrl: trackingUrl,
                            etiquetaLogistica: etiquetaLogistica,
                            nombresProductos: nombresProductos
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enviando mail (Despachando): {ex.Message}");
                    }
                }
            }

            return resultado;
        }

        // 3. CAMBIO DE ESTADO FINAL (CADETE) - Entregado (7) o Entrega fallida (8)
        public async Task<bool> CambiarEstadoAsync(ChangeOrderStatusDTO changeStatusDto)
        {
            // Trae el pedido incluyendo Cliente y Detalles (para tener el mail y productos)
            var pedido = await _orderRepository.GetByIdWithClienteAndDetailsAsync(changeStatusDto.IDPedido);
            if (pedido == null) return false;

            // Capturar el estado anterior antes de cambios
            int estadoAnterior = pedido.IDEstadoDePedido;

            // === VALIDACIÓN DE TRANSICIONES DE ESTADO ===
            // CU25: Operario: 1 (Sin preparar) -> 2 (Preparando)
            // CU25: Operario: 2 (Preparando) -> 4 (Listo para despachar)
            // CU25: Operario: 2 -> 2 (Solo registrar inicio de armado)
            // Cadete: 6 (En camino) -> 7 (Entregado) o 8 (Entrega fallida)
            // Admin: Cualquier estado -> 9 (Cancelado)
            
            bool transicionValida = false;
            
            // CANCELACIÓN: Admin puede cancelar desde cualquier estado excepto terminales (7=Entregado, 9=Cancelado)
            if (changeStatusDto.IDNuevoEstado == 9 && pedido.IDEstadoDePedido != 7 && pedido.IDEstadoDePedido != 9)
                transicionValida = true;
            // Operario asignado pasando de Sin preparar a Preparar
            else if (pedido.IDEstadoDePedido == 1 && changeStatusDto.IDNuevoEstado == 2)
                transicionValida = true;
            // ✅ CU25: Operario registrando que inicia armado (2→2, solo actualiza FechaInicioArmado)
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 2)
                transicionValida = true;
            // Operario finalizando preparación
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 4)
                transicionValida = true;
            // Cadete: Transiciones permitidas para entregas
            // 5 (Despachando) → 6 (En camino), reintentando desde 8 (Fallo)
            else if ((pedido.IDEstadoDePedido == 5 || pedido.IDEstadoDePedido == 6 || pedido.IDEstadoDePedido == 8) && 
                     (changeStatusDto.IDNuevoEstado == 6 || changeStatusDto.IDNuevoEstado == 7 || changeStatusDto.IDNuevoEstado == 8))
                transicionValida = true;

            if (!transicionValida) return false;

            int estadoFinal = changeStatusDto.IDNuevoEstado;
            int? intentoEntrega = null;

            // ✅ CU25: Cuando operario inicia armado (1→2), guardar FechaInicioArmado
            if (pedido.IDEstadoDePedido == 1 && changeStatusDto.IDNuevoEstado == 2)
            {
                pedido.FechaInicioArmado = DateTime.Now;
            }
            // ✅ CU25: Cuando operario presiona "Comenzar armado" (2→2), guardar FechaInicioArmado si no existe
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 2 && !pedido.FechaInicioArmado.HasValue)
            {
                pedido.FechaInicioArmado = DateTime.Now;
            }
            // ✅ CU25: Cuando operario finaliza armado (2→4), guardar FechaFinArmado
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 4)
            {
                pedido.FechaFinArmado = DateTime.Now;
            }
            else if (changeStatusDto.IDNuevoEstado == 7)
            {
                pedido.FechaEntregaReal = DateTime.Now;
                pedido.HoraEntregaReal = DateTime.Now.TimeOfDay;
                pedido.IntentosEntregaFallida = 0; // Resetear intentos si se entrega exitosamente
            }
            else if (changeStatusDto.IDNuevoEstado == 8)
            {
                // Manejar lógica de intentos fallidos
                pedido.IntentosEntregaFallida++;
                intentoEntrega = pedido.IntentosEntregaFallida;

                // Si supera 3 intentos, cambiar a estado 9 (Cancelado automáticamente)
                if (pedido.IntentosEntregaFallida >= 3)
                {
                    pedido.IDEstadoDePedido = 9;
                    pedido.EstadoActual = "Cancelado automáticamente";
                    pedido.JustificacionCancelacion = "Superó los 3 intentos fallidos.";
                    estadoFinal = 9;  // ✅ Guardar estado 9 en el historial
                }
                else
                {
                    // ✅ NUEVO: El historial registra estado 8, el pedido vuelve a estado 5 para reintento
                    pedido.IDEstadoDePedido = 5;
                    pedido.EstadoActual = "Despachando";
                    estadoFinal = 8;  // Guardar estado 8 en el historial
                }
            }
            else
            {
                // ✅ NUEVA: Actualizar ambos IDEstadoDePedido y EstadoActual consistentemente
                pedido.IDEstadoDePedido = changeStatusDto.IDNuevoEstado;
                pedido.EstadoActual = ObtenerDescripcionEstado(changeStatusDto.IDNuevoEstado);
            }

            var nuevoHistorial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = estadoFinal,
                IDUsuario = changeStatusDto.IDUsuario,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = changeStatusDto.IDNuevoEstado == 8
                                ? changeStatusDto.MotivoCancelacion
                                : (changeStatusDto.Observaciones ?? "Estado actualizado por el cadete."),
                IntentosEntregaFallida = (estadoFinal == 8 || estadoFinal == 9) ? pedido.IntentosEntregaFallida : 0,
                IntentosMax = 3
            };

            // Pasar el pedido actualizado al repositorio para que guarde todos los cambios
            var resultado = await _repository.ActualizarEstadoAsync(nuevoHistorial, pedido);

            // Envío automático de mail al cliente si el cambio de estado fue exitoso
            // NO enviar correo si es transición 1→2 o 2→2 (ya lo hace AsignarOperarioAsync en 1→2, y 2→2 es solo registrar inicio armado)
            bool debeEnviarCorreo = resultado && !(estadoFinal == 2);
            
            if (debeEnviarCorreo)
            {
                var destinatario = pedido.Cliente?.Mail; // Usa 'Email' si tu modelo lo llama así
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre  // Solo el nombre, como en "sin preparar"
                    : "Cliente";
                
                // Usar el estado final para la descripción (puede ser 9 si fueron 3 intentos fallidos)
                var estadoDescripcion = ObtenerDescripcionEstado(estadoFinal);

                // Obtener etiqueta logística
                var etiquetaLogistica = await _clientProductRelationService.ObtenerEtiquetaLogisticaAsync(
                    pedido.IDCliente, 
                    pedido.IDPedido);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        var trackingUrl = GenerarTrackingUrl(pedido.IDPedido);
                        var nombresProductos = ObtenerNombresProductos(pedido);
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            estadoFinal,
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP",
                            intentoEntrega: intentoEntrega, // Pasar el número de intento si es entrega fallida
                            intentosMax: 3,
                            trackingUrl: trackingUrl,
                            etiquetaLogistica: etiquetaLogistica,
                            nombresProductos: nombresProductos
                        );
                        Console.WriteLine($"[EmailSender] Email enviado correctamente al cliente: {destinatario}");
                    }
                    catch (Exception ex)
                    {
                        // Log y no romper la petición
                        Console.WriteLine($"[ERROR] Error enviando mail: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"[ADVERTENCIA] El pedido {pedido.IDPedido} no tiene email de cliente cargado, no se envió notificación.");
                }
            }

            return resultado;
        }

        // 4. CANCELAR PEDIDO (ADMIN/OPERARIO) - Pasa a Cancelado (10)
        public async Task<bool> CancelarPedidoAsync(CancelarPedidoDTO dto, int userId)
        {
            var pedido = await _orderRepository.GetByIdWithClienteAsync(dto.PedidoId);

            // No se puede cancelar si ya está entregado (7) o ya cancelado (9)
            if (pedido == null || pedido.IDEstadoDePedido == 7 || pedido.IDEstadoDePedido == 9)
                return false;

            pedido.IDEstadoDePedido = 9; // Estado Cancelado
            pedido.MotivoCancelacionId = dto.MotivoCancelacionId;
            pedido.JustificacionCancelacion = dto.Justificacion;

            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 9,
                IDUsuario = userId, // El ID del Admin/Operario que cancela
                fecha_hora_inicio = DateTime.Now,
                // Registramos el ID del motivo y la justificación en las observaciones
                Observaciones = $"Cancelación (Motivo ID: {dto.MotivoCancelacionId}). Justificación: {dto.Justificacion ?? "Sin justificación adicional."}"
            };

            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(10);

                // Obtener etiqueta logística
                var etiquetaLogistica = await _clientProductRelationService.ObtenerEtiquetaLogisticaAsync(
                    pedido.IDCliente, 
                    pedido.IDPedido);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        var trackingUrl = GenerarTrackingUrl(pedido.IDPedido);
                        var nombresProductos = ObtenerNombresProductos(pedido);
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            5,                                  // Despachando
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP",
                            trackingUrl: trackingUrl,
                            etiquetaLogistica: etiquetaLogistica,
                            nombresProductos: nombresProductos
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enviando mail (Cancelado): {ex.Message}");
                    }
                }
            }

            return resultado;
        }

        public async Task<bool> CancelarPedidoAsync(CancelarPedidoDTO dto)
        {
            var pedido = await _orderRepository.GetByIdWithClienteAndDetailsAsync(dto.PedidoId);

            if (pedido == null || pedido.IDEstadoDePedido == 7 || pedido.IDEstadoDePedido == 9)
                return false;

            pedido.IDEstadoDePedido = 9;
            pedido.MotivoCancelacionId = dto.MotivoCancelacionId;
            pedido.JustificacionCancelacion = dto.Justificacion;

            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 9,
                IDUsuario = pedido.IDUsuario, // Usuario responsable registrado
                fecha_hora_inicio = DateTime.Now,
                Observaciones = $"Cancelación. Motivo ID: {dto.MotivoCancelacionId}. Justificación: {dto.Justificacion ?? "Sin detalle."}"
            };

            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre  // Solo el nombre, como en "sin preparar"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(10);

                // Obtener etiqueta logística
                var etiquetaLogistica = await _clientProductRelationService.ObtenerEtiquetaLogisticaAsync(
                    pedido.IDCliente, 
                    pedido.IDPedido);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        var trackingUrl = GenerarTrackingUrl(pedido.IDPedido);
                        var nombresProductos = ObtenerNombresProductos(pedido);
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            10,                                 // Cancelado
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP",
                            trackingUrl: trackingUrl,
                            etiquetaLogistica: etiquetaLogistica,
                            nombresProductos: nombresProductos
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enviando mail (Cancelado - sin userId): {ex.Message}");
                    }
                }
            }

            return resultado;
        }

        // Utilidad: Descripción legible del estado
        private string ObtenerDescripcionEstado(int idEstado)
        {
            return idEstado switch
            {
                1 => "Sin preparar",
                2 => "Preparar pedido",
                3 => "Demorado",
                4 => "Listo para despachar",
                5 => "Despachando",
                6 => "En camino",
                7 => "Entregado",
                8 => "Entrega fallida",
                9 => "Cancelado automáticamente",
                10 => "Cancelado",
                _ => "Desconocido"
            };
        }
    }
}