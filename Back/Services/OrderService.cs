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

        public OrderService(IOrderRepository orderRepository, IClientRepository clientRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<int> CreateOrderAsync(CreateOrderDTO orderDto)
        {
            int idClienteFinal = orderDto.IDCliente ?? 0;

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
            }

            // Convertimos DTO a Modelo con el cliente resuelto
            var pedido = _mapper.Map<Pedido>(orderDto);
            pedido.IDCliente = idClienteFinal;

            // Si viene dirección del cliente nuevo, guardarla
            if (!string.IsNullOrEmpty(orderDto.Direccion))
            {
                pedido.DireccionEntrega = orderDto.Direccion;
            }

            // Lógica del Mandato: Registro inicial en el historial para trazabilidad
            var historialInicial = new HistorialDeEstados
            {
                IDEstadoDePedido = 1, // "Sin preparar"
                fecha_hora_inicio = DateTime.Now,
                IDUsuario = orderDto.IDUsuario,
                Observaciones = "Pedido recibido e ingresado al sistema."
            };

            pedido.HistorialDeEstados.Add(historialInicial);

            // El repo maneja la transacción y el guardado de detalles (RF17)
            return await _orderRepository.CreateOrderAsync(pedido);
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