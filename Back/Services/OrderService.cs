using AutoMapper;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Back.Services.Interfaces;

namespace Back.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public OrderService(
            IOrderRepository orderRepository, 
            IClientRepository clientRepository, 
            IMapper mapper,
            EmailSender emailSender,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<int> CreateOrderAsync(CreateOrderDTO orderDto)
        {
            int idClienteFinal = orderDto.IDCliente ?? 0;
            string clientEmail = string.Empty;
            string clienteName = string.Empty;

            // Si no hay cliente, crear uno nuevo
            if (idClienteFinal <= 0)
            {
                if (string.IsNullOrEmpty(orderDto.NombreCliente) || 
                    string.IsNullOrEmpty(orderDto.Telefono) ||
                    string.IsNullOrEmpty(orderDto.Email))
                {
                    throw new ArgumentException("Para crear un cliente nuevo, se requieren Nombre, Teléfono y Email");
                }

                // Crear cliente nuevo
                var nuevoCliente = new Cliente
                {
                    Nombre = orderDto.NombreCliente.Split(' ').FirstOrDefault() ?? "",
                    Apellido = string.Join(" ", orderDto.NombreCliente.Split(' ').Skip(1)),
                    Telefono = orderDto.Telefono,
                    Mail = orderDto.Email,
                    DNI = "SIN_DNI_" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    Direccion = orderDto.Direccion ?? "",
                    IDLocalidad = 1, // Localidad por defecto
                    IDBarrio = 1 // Barrio por defecto
                };

                await _clientRepository.AddAsync(nuevoCliente);

                idClienteFinal = nuevoCliente.IDCliente;
                clientEmail = nuevoCliente.Mail;
                clienteName = nuevoCliente.Nombre;
            }
            else
            {
                // Obtener los datos del cliente existente
                var clienteExistente = await _clientRepository.GetByIdAsync(idClienteFinal);
                if (clienteExistente != null)
                {
                    clientEmail = clienteExistente.Mail;
                    clienteName = clienteExistente.Nombre;
                }
            }

            // Convertimos DTO a Modelo con el cliente resuelto
            var pedido = _mapper.Map<Pedido>(orderDto);
            pedido.IDCliente = idClienteFinal;

            // Si viene dirección del cliente nuevo, guardarla
            if (!string.IsNullOrEmpty(orderDto.Direccion))
            {
                pedido.DireccionEntrega = orderDto.Direccion;
            }

            // El repo maneja el registro en el historial (RF17)
            int pedidoId = await _orderRepository.CreateOrderAsync(pedido, orderDto.IDUsuario);

            // ✅ Construir el tracking URL y enviar email
            await EnviarEmailTrackingAsync(pedidoId, clientName: clienteName, clientEmail: clientEmail);

            return pedidoId;
        }

        /// <summary>
        /// Envía email de confirmación de pedido al cliente cuando se crea
        /// </summary>
        private async Task EnviarEmailTrackingAsync(int pedidoId, string clientName, string clientEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(clientEmail))
                {
                    Console.WriteLine($"[OrderService] No hay email disponible para el pedido #{pedidoId}");
                    return;
                }

                // Obtener la URL base de la aplicación desde configuración o variable de entorno
                var baseUrl = _configuration["AppSettings:FrontendUrl"] 
                    ?? Environment.GetEnvironmentVariable("FRONTEND_URL") 
                    ?? "https://midominio.com";

                // Eliminar trailing slash si existe
                if (baseUrl.EndsWith("/"))
                    baseUrl = baseUrl.TrimEnd('/');

                // Construir el URL único de seguimiento
                var trackingUrl = $"{baseUrl}/tracking/{pedidoId}";

                Console.WriteLine($"[OrderService] Enviando email de creación de pedido #{pedidoId} a {clientEmail}");
                Console.WriteLine($"[OrderService] URL de tracking: {trackingUrl}");

                // ✅ Usar el mismo método que AsignarOperarioAsync - es más robusto
                // Estado 1 = Pedido Creado / Sin preparar
                await _emailSender.EnviarCorreoCambioEstadoHtml(
                    destinatario: clientEmail,
                    nombreCliente: clientName,
                    estadoDescripcion: "Pedido Recibido",
                    numeroPedido: pedidoId,
                    idEstado: 1,  // Sin preparar (Recibido)
                    brandName: "Farmacia General Paz",
                    supportEmail: "contacto@farmaciageneralpaz.com",
                    brandCode: "FGP",
                    trackingUrl: trackingUrl
                );

                Console.WriteLine($"[OrderService] Email enviado exitosamente para pedido #{pedidoId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] Error al enviar email de creación: {ex.Message}");
                // No relanzar la excepción para que el pedido se cree igualmente
            }
        }

        public async Task<OrderTrackingDTO> GetOrderTrackingAsync(int id)
        {
            var pedido = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (pedido == null) return null;

            return _mapper.Map<OrderTrackingDTO>(pedido);
        }

        public async Task<bool> UpdateOrderStatusAsync(ChangeOrderStatusDTO changeDto)
        {
            // Lógica para futuros cambios de estado
            throw new NotImplementedException();
        }
    }
}