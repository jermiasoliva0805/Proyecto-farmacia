using AutoMapper;
using Back.Data;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Back.Services.Interfaces;
using Back.Services; // EmailSender
using Microsoft.EntityFrameworkCore;
 
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
        private readonly AppDbContext _context;
 
        public OrderStatusService(
            IOrderStatusRepository repository,
            IOrderRepository orderRepository,
            IMapper mapper,
            EmailSender emailSender,
            IConfiguration configuration,
            ClientProductRelationService clientProductRelationService,
            AppDbContext context)
        {
            _repository = repository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailSender = emailSender;
            _configuration = configuration;
            _clientProductRelationService = clientProductRelationService;
            _context = context;
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
 
        // 1. ASIGNAR OPERARIO (encargado) - Pasa de Sin preparar (1) a Preparar pedido (2)
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
                fecha_hora_inicio = DateTime.UtcNow,
                Observaciones = "Encargado asignó operario para la preparación del pedido."
            };
 
            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);
 
            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(2);
 
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
                            2,
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
 
        // 2. ASIGNAR CADETE (ENCARGADO) - Pasa de Listo para despachar (4) a Despachando (5)
        public async Task<bool> AsignarCadeteAsync(AssignDeliveryDTO dto)
        {
            // Trae Cliente y Detalles para poder enviar correo personalizado
            var pedido = await _orderRepository.GetByIdWithClienteAndDetailsAsync(dto.PedidoId);
 
            // El pedido debe estar en 4 (Listo para despachar) para pasar a 5 (Despachando)
            if (pedido == null || pedido.IDEstadoDePedido != 4) return false;
 
            pedido.IDEstadoDePedido = 5; // Despachando
            pedido.IDUsuario = dto.CadeteId;
 
            // ✅ FIX: Solo asignar zona del cadete si el pedido NO tiene zona ya asignada
            if (!pedido.ZonaId.HasValue)
            {
                var cadete = await _context.Usuarios.FindAsync(dto.CadeteId);
                if (cadete != null && cadete.ZonaId.HasValue)
                {
                    pedido.ZonaId = cadete.ZonaId;
                    Console.WriteLine($"[AsignarCadete] Pedido #{pedido.IDPedido} - ZonaId asignada como fallback desde cadete: {cadete.ZonaId} ({cadete.Nombre})");
                }
                else
                {
                    Console.WriteLine($"[AsignarCadete] ADVERTENCIA: El cadete ID {dto.CadeteId} no tiene zona asignada. El pedido #{pedido.IDPedido} quedará sin zona.");
                }
            }
            else
            {
                Console.WriteLine($"[AsignarCadete] Pedido #{pedido.IDPedido} ya tiene ZonaId={pedido.ZonaId}, no se sobreescribe.");
            }
 
            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 5,
                IDUsuario = dto.CadeteId,
                fecha_hora_inicio = DateTime.UtcNow,
                Observaciones = "Cadete asignado. Pedido en despachando."
            };
 
            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);
 
            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(5);
 
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
                            5,
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
            var pedido = await _orderRepository.GetByIdWithClienteAndDetailsAsync(changeStatusDto.IDPedido);
            if (pedido == null) return false;
 
            int estadoAnterior = pedido.IDEstadoDePedido;
 
            bool transicionValida = false;
 
            if (changeStatusDto.IDNuevoEstado == 9 && pedido.IDEstadoDePedido != 7 && pedido.IDEstadoDePedido != 9)
                transicionValida = true;
            else if (pedido.IDEstadoDePedido == 1 && changeStatusDto.IDNuevoEstado == 2)
                transicionValida = true;
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 2)
                transicionValida = true;
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 4)
                transicionValida = true;
            else if (pedido.IDEstadoDePedido == 3 &&
                     (changeStatusDto.IDNuevoEstado == 4 ||
                      changeStatusDto.IDNuevoEstado == 5 ||
                      changeStatusDto.IDNuevoEstado == 6 ||
                      changeStatusDto.IDNuevoEstado == 7 ||
                      changeStatusDto.IDNuevoEstado == 8))
                transicionValida = true;
            else if ((pedido.IDEstadoDePedido == 5 || pedido.IDEstadoDePedido == 6 || pedido.IDEstadoDePedido == 8) &&
                     (changeStatusDto.IDNuevoEstado == 6 || changeStatusDto.IDNuevoEstado == 7 || changeStatusDto.IDNuevoEstado == 8))
                transicionValida = true;
 
            if (!transicionValida) return false;
 
            int estadoFinal = changeStatusDto.IDNuevoEstado;
            int? intentoEntrega = null;
 
            if (pedido.IDEstadoDePedido == 1 && changeStatusDto.IDNuevoEstado == 2)
            {
                pedido.FechaInicioArmado = DateTime.UtcNow;
            }
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 2 && !pedido.FechaInicioArmado.HasValue)
            {
                pedido.FechaInicioArmado = DateTime.UtcNow;
            }
            else if (pedido.IDEstadoDePedido == 2 && changeStatusDto.IDNuevoEstado == 4)
            {
                pedido.FechaFinArmado = DateTime.UtcNow;
            }
            else if (pedido.IDEstadoDePedido == 3 &&
                     (changeStatusDto.IDNuevoEstado == 4 ||
                      changeStatusDto.IDNuevoEstado == 5 ||
                      changeStatusDto.IDNuevoEstado == 6 ||
                      changeStatusDto.IDNuevoEstado == 7 ||
                      changeStatusDto.IDNuevoEstado == 8))
            {
                pedido.IDEstadoDePedido = changeStatusDto.IDNuevoEstado;
                pedido.EstadoActual = ObtenerDescripcionEstado(changeStatusDto.IDNuevoEstado);
                pedido.Estado = ObtenerDescripcionEstado(changeStatusDto.IDNuevoEstado);
            }
            else if (changeStatusDto.IDNuevoEstado == 7)
            {
                pedido.FechaEntregaReal = DateTime.UtcNow;
                pedido.HoraEntregaReal = DateTime.UtcNow.TimeOfDay;
                pedido.IntentosEntregaFallida = 0;
                pedido.EstadoActual = ObtenerDescripcionEstado(7);
                pedido.Estado = ObtenerDescripcionEstado(7);
            }
            else if (changeStatusDto.IDNuevoEstado == 8)
            {
                pedido.IntentosEntregaFallida++;
                intentoEntrega = pedido.IntentosEntregaFallida;
 
                if (pedido.IntentosEntregaFallida >= 3)
                {
                    pedido.IDEstadoDePedido = 9;
                    pedido.EstadoActual = "Cancelado automáticamente";
                    pedido.Estado = ObtenerDescripcionEstado(9);
                    pedido.JustificacionCancelacion = "Superó los 3 intentos fallidos.";
                    estadoFinal = 9;
                }
                else
                {
                    pedido.IDEstadoDePedido = 5;
                    pedido.EstadoActual = "Despachando";
                    estadoFinal = 8;
                }
            }
            else
            {
                pedido.IDEstadoDePedido = changeStatusDto.IDNuevoEstado;
                pedido.EstadoActual = ObtenerDescripcionEstado(changeStatusDto.IDNuevoEstado);
            }
 
            // Guardar responsable del último cambio para mantener trazabilidad y filtros por usuario
            pedido.IDUsuario = changeStatusDto.IDUsuario;
 
            var nuevoHistorial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = estadoFinal,
                IDUsuario = changeStatusDto.IDUsuario,
                fecha_hora_inicio = DateTime.UtcNow,
                Observaciones = changeStatusDto.IDNuevoEstado == 8
                                ? changeStatusDto.MotivoCancelacion
                                : (changeStatusDto.Observaciones ?? "Estado actualizado por el cadete."),
                IntentosEntregaFallida = (estadoFinal == 8 || estadoFinal == 9) ? pedido.IntentosEntregaFallida : 0,
                IntentosMax = 3
            };
 
            var resultado = await _repository.ActualizarEstadoAsync(nuevoHistorial, pedido);
 
            bool debeEnviarCorreo = resultado && !(estadoFinal == 2);
 
            if (debeEnviarCorreo)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre
                    : "Cliente";
 
                var estadoDescripcion = ObtenerDescripcionEstado(estadoFinal);
 
                var etiquetaLogistica = await _clientProductRelationService.ObtenerEtiquetaLogisticaAsync(
                    pedido.IDCliente,
                    pedido.IDPedido);
 
                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        var trackingUrl = GenerarTrackingUrl(pedido.IDPedido);
                        var nombresProductos = ObtenerNombresProductos(pedido);
                        var surveyUrl = estadoFinal == 7
                            ? $"https://docs.google.com/forms/d/e/1FAIpQLSd-5dZ-nXPZQki795XHgcZwZOVo-J0Q9H89MtuBDljFlMV0xg/viewform?usp=pp_url&entry.1385219541={Uri.EscapeDataString(destinatario)}"
                            : null;
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            estadoFinal,
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP",
                            intentoEntrega: intentoEntrega,
                            intentosMax: 3,
                            trackingUrl: trackingUrl,
                            etiquetaLogistica: etiquetaLogistica,
                            nombresProductos: nombresProductos,
                            surveyUrl: surveyUrl
                        );
                        Console.WriteLine($"[EmailSender] Email enviado correctamente al cliente: {destinatario}");
                    }
                    catch (Exception ex)
                    {
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
 
            if (pedido == null || pedido.IDEstadoDePedido == 7 || pedido.IDEstadoDePedido == 9)
                return false;
 
            pedido.IDEstadoDePedido = 9;
            pedido.MotivoCancelacionId = dto.MotivoCancelacionId;
            pedido.JustificacionCancelacion = dto.Justificacion;
            pedido.Estado = ObtenerDescripcionEstado(9);
 
            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 9,
                IDUsuario = userId,
                fecha_hora_inicio = DateTime.UtcNow,
                Observaciones = $"Cancelación (Motivo ID: {dto.MotivoCancelacionId}). Justificación: {dto.Justificacion ?? "Sin justificación adicional."}"
            };
 
            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);
 
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(10);
 
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
                            9,
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
            pedido.Estado = ObtenerDescripcionEstado(9);
 
            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 9,
                IDUsuario = pedido.IDUsuario,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = $"Cancelación. Motivo ID: {dto.MotivoCancelacionId}. Justificación: {dto.Justificacion ?? "Sin detalle."}"
            };
 
            var resultado = await _repository.ActualizarEstadoAsync(historial, pedido);
 
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? pedido.Cliente.Nombre
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(10);
 
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
                            10,
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