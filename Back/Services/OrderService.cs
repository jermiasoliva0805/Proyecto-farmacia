using Back.DTOs;
using Back.Services.Interfaces;
using Back.Repositories.Interfaces;
using Back.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Linq;

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
            
            // 🔴 IMPORTANTE: Asignar ZonaId si viene en el DTO
            if (orderDto.ZonaId.HasValue && orderDto.ZonaId.Value > 0)
            {
                pedido.ZonaId = orderDto.ZonaId;
                Console.WriteLine($"[OrderService] Zona asignada al pedido: {orderDto.ZonaId}");
            }
            else
            {
                Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: El pedido se crea sin zona (ZonaId = {orderDto.ZonaId})");
            }

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
                    ?? "https://farmacia-app.com";

                var trackingUrl = $"{baseUrl}/seguimiento/{pedidoId}";

                var subject = $"Tu pedido #{pedidoId} ha sido recibido";
                var body = $@"
                    <h2>¡Hola {clientName}!</h2>
                    <p>Tu pedido ha sido recibido correctamente.</p>
                    <p><strong>Número de Pedido:</strong> #{pedidoId}</p>
                    <p>Puedes seguir el estado de tu pedido aquí:</p>
                    <p><a href='{trackingUrl}'>Ver seguimiento del pedido</a></p>
                    <br/>
                    <p>¡Gracias por tu compra!</p>
                ";

                await _emailSender.SendEmailAsync(clientEmail, subject, body);
                Console.WriteLine($"[OrderService] Email de tracking enviado a {clientEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] Error al enviar email de tracking: {ex.Message}");
                // No lanzamos la excepción, solo la registramos
            }
        }

        public async Task<OrderTrackingDTO> GetOrderTrackingAsync(int id)
        {
            // Implementar según necesidad
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateOrderStatusAsync(ChangeOrderStatusDTO changeDto)
        {
            // Implementar según necesidad
            throw new NotImplementedException();
        }
    }
}
