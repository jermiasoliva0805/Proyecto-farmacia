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

        public OrderStatusService(
            IOrderStatusRepository repository,
            IOrderRepository orderRepository,
            IMapper mapper,
            EmailSender emailSender)
        {
            _repository = repository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailSender = emailSender;
        }

        // 1. ASIGNAR OPERARIO (ADMIN) - Pasa de Sin preparar (1) a Preparar pedido (2)
        public async Task<bool> AsignarOperarioAsync(AssignOperatorDTO dto)
        {
            // Trae Cliente para poder enviar correo
            var pedido = await _orderRepository.GetByIdWithClienteAsync(dto.PedidoId);

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

            var resultado = await _repository.ActualizarEstadoAsync(historial);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(2);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            2,                                  // Preparar pedido
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP"
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

        // 2. ASIGNAR CADETE (ADMIN) - Pasa de Listo para despachar (4) a En camino (6)
        public async Task<bool> AsignarCadeteAsync(AssignDeliveryDTO dto)
        {
            // Trae Cliente para poder enviar correo
            var pedido = await _orderRepository.GetByIdWithClienteAsync(dto.PedidoId);

            // El pedido debe estar en 4 (Listo para despachar) para pasar a 6 (En camino)
            if (pedido == null || pedido.IDEstadoDePedido != 4) return false;

            pedido.IDEstadoDePedido = 6; // En camino
            pedido.IDUsuario = dto.CadeteId;

            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 6,
                IDUsuario = dto.CadeteId,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = "Admin asignó cadete. Pedido en camino al domicilio."
            };

            var resultado = await _repository.ActualizarEstadoAsync(historial);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(6);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            6,                                  // En camino
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enviando mail (En camino): {ex.Message}");
                    }
                }
            }

            return resultado;
        }

        // 3. CAMBIO DE ESTADO FINAL (CADETE) - Entregado (7) o Entrega fallida (8)
        public async Task<bool> CambiarEstadoAsync(ChangeOrderStatusDTO changeStatusDto)
        {
            // Trae el pedido incluyendo Cliente (para tener el mail)
            var pedido = await _orderRepository.GetByIdWithClienteAsync(changeStatusDto.IDPedido);
            if (pedido == null) return false;

            // REGLA: Finaliza solo si está "En camino" (6)
            if (pedido.IDEstadoDePedido != 6) return false;

            if (changeStatusDto.IDNuevoEstado == 7)
            {
                pedido.FechaEntregaReal = DateTime.Now;
                pedido.HoraEntregaReal = DateTime.Now.TimeOfDay;
            }

            pedido.IDEstadoDePedido = changeStatusDto.IDNuevoEstado;

            var nuevoHistorial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = changeStatusDto.IDNuevoEstado,
                IDUsuario = changeStatusDto.IDUsuario,
                fecha_hora_inicio = DateTime.Now,
                Observaciones = changeStatusDto.IDNuevoEstado == 8
                                ? changeStatusDto.MotivoCancelacion
                                : (changeStatusDto.Observaciones ?? "Estado actualizado por el cadete.")
            };

            var resultado = await _repository.ActualizarEstadoAsync(nuevoHistorial);

            // Envío automático de mail al cliente si el cambio de estado fue exitoso
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail; // Usa 'Email' si tu modelo lo llama así
                var nombreCliente = pedido.Cliente != null
                    ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(changeStatusDto.IDNuevoEstado);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        // Para Entrega Fallida (8) puedes pasar intentoEntrega si lo manejas; aquí va null por defecto
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            changeStatusDto.IDNuevoEstado,
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP",
                            intentoEntrega: null, // coloca el intento real si tienes esa lógica
                            intentosMax: 3
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log y no romper la petición
                        Console.WriteLine($"Error enviando mail: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Aviso: El pedido no tiene email de cliente cargado, no se envió notificación.");
                }
            }

            return resultado;
        }

        // 4. CANCELAR PEDIDO (ADMIN/OPERARIO) - Pasa a Cancelado (10)
        public async Task<bool> CancelarPedidoAsync(CancelarPedidoDTO dto, int userId)
        {
            var pedido = await _orderRepository.GetByIdWithClienteAsync(dto.PedidoId);

            // No se puede cancelar si ya está entregado (7) o ya cancelado (10)
            if (pedido == null || pedido.IDEstadoDePedido == 7 || pedido.IDEstadoDePedido == 10)
                return false;

            pedido.IDEstadoDePedido = 10; // Estado Cancelado

            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 10,
                IDUsuario = userId, // El ID del Admin/Operario que cancela
                fecha_hora_inicio = DateTime.Now,
                // Registramos el ID del motivo y la justificación en las observaciones
                Observaciones = $"Cancelación (Motivo ID: {dto.MotivoCancelacionId}). Justificación: {dto.Justificacion ?? "Sin justificación adicional."}"
            };

            var resultado = await _repository.ActualizarEstadoAsync(historial);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(10);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            10,                                 // Cancelado
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP"
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
            var pedido = await _orderRepository.GetByIdWithClienteAsync(dto.PedidoId);

            if (pedido == null || pedido.IDEstadoDePedido == 7 || pedido.IDEstadoDePedido == 10)
                return false;

            pedido.IDEstadoDePedido = 10;
            pedido.MotivoCancelacionId = dto.MotivoCancelacionId;
            pedido.JustificacionCancelacion = dto.Justificacion;

            var historial = new HistorialDeEstados
            {
                IDPedido = pedido.IDPedido,
                IDEstadoDePedido = 10,
                IDUsuario = pedido.IDUsuario, // Usuario responsable registrado
                fecha_hora_inicio = DateTime.Now,
                Observaciones = $"Cancelación. Motivo ID: {dto.MotivoCancelacionId}. Justificación: {dto.Justificacion ?? "Sin detalle."}"
            };

            var resultado = await _repository.ActualizarEstadoAsync(historial);

            // Notificación por email
            if (resultado)
            {
                var destinatario = pedido.Cliente?.Mail;
                var nombreCliente = pedido.Cliente != null
                    ? $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}"
                    : "Cliente";
                var estadoDescripcion = ObtenerDescripcionEstado(10);

                if (!string.IsNullOrWhiteSpace(destinatario))
                {
                    try
                    {
                        await _emailSender.EnviarCorreoCambioEstadoHtml(
                            destinatario,
                            nombreCliente,
                            estadoDescripcion,
                            pedido.IDPedido,
                            10,                                 // Cancelado
                            "Farmacia General Paz",
                            "contacto@farmaciageneralpaz.com",
                            "FGP"
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
                10 => "Cancelado",
                _ => "Desconocido"
            };
        }
    }
}