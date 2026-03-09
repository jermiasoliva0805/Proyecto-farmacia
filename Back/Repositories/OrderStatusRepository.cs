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
                pedido.EstadoActual = "Cancelado";

                // Si se proporciona un pedido actualizado, copiar campos importantes
                if (pedidoActualizado != null)
                {
                    pedido.MotivoCancelacionId = pedidoActualizado.MotivoCancelacionId;
                    pedido.JustificacionCancelacion = pedidoActualizado.JustificacionCancelacion;
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
    }
}