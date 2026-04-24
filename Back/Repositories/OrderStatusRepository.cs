using Back.Data;
using Back.Models;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Back.Repositories
{
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly AppDbContext _context; 

        public OrderStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarEstadoAsync(HistorialDeEstados nuevoHistorial, Pedido pedidoActualizado = null)
        {
            try
            {
                var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IDPedido == nuevoHistorial.IDPedido);
                if (pedido == null) return false;

                // Actualizar el estado del pedido
                pedido.IDEstadoDePedido = nuevoHistorial.IDEstadoDePedido;

                // Si se proporciona un pedido actualizado, copiar todos los campos importantes
                if (pedidoActualizado != null)
                {
                    pedido.EstadoActual = pedidoActualizado.EstadoActual;
                    pedido.Estado = pedidoActualizado.Estado;
                    pedido.IDUsuario = pedidoActualizado.IDUsuario;
                    pedido.ZonaId = pedidoActualizado.ZonaId;
                    pedido.MotivoCancelacionId = pedidoActualizado.MotivoCancelacionId;
                    pedido.JustificacionCancelacion = pedidoActualizado.JustificacionCancelacion;
                    pedido.FechaInicioArmado = pedidoActualizado.FechaInicioArmado;
                    pedido.FechaFinArmado = pedidoActualizado.FechaFinArmado;
                    pedido.FechaEntregaReal = pedidoActualizado.FechaEntregaReal;
                    pedido.HoraEntregaReal = pedidoActualizado.HoraEntregaReal;
                    pedido.IntentosEntregaFallida = pedidoActualizado.IntentosEntregaFallida;
                }
                else
                {
                    // Fallback: si no se proporciona pedido actualizado, usar descripción estándar
                    pedido.EstadoActual = ObtenerDescripcionEstado(nuevoHistorial.IDEstadoDePedido);
                }

                _context.Pedidos.Update(pedido);
                _context.HistorialesDeEstados.Add(nuevoHistorial);

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error en ActualizarEstadoAsync: {ex.Message}");
                throw;
            }
        }

        // Helper para obtener la descripción del estado
        private string ObtenerDescripcionEstado(int idEstado)
        {
            return idEstado switch
            {
                1 => "Sin preparar",
                2 => "En Armado",
                3 => "Preparado",
                4 => "Listo para Despachar",
                5 => "Despachando",
                6 => "En camino",
                7 => "Entregado",
                8 => "Fallo en entrega",
                9 => "Cancelado",
                10 => "Cancelado",
                _ => "Estado desconocido"
            };
        }
    }
}
