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
            var pedido = pedidoActualizado ?? await _context.Pedidos.FindAsync(nuevoHistorial.IDPedido);
            if (pedido == null) return false;

            // Actualizar el estado del pedido
            pedido.IDEstadoDePedido = nuevoHistorial.IDEstadoDePedido;

            // Si se proporciona un pedido actualizado, guardar todos los cambios (intentos, fecha real, etc.)
            if (pedidoActualizado != null)
            {
                _context.Entry(pedido).State = EntityState.Modified;
            }

            // Nombre de la tabla según tu AppDbContext: HistorialesDeEstados
            _context.HistorialesDeEstados.Add(nuevoHistorial);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}