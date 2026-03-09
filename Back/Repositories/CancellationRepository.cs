using Back.Models;
using Back.Interfaces;
using Back.Data;
using Microsoft.EntityFrameworkCore;

namespace Back.Repositories
{
    public class CancellationRepository : ICancellationRepository
    {
        private readonly AppDbContext _context;

        public CancellationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CancelOrderAsync(int idPedido, int? motivoCancelacionId = null, string justificacion = "")
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IDPedido == idPedido);
            if (pedido == null) return;

            // Actualizar campos del pedido
            pedido.IDEstadoDePedido = 9; // Estado "Cancelado"
            pedido.EstadoActual = "Cancelado";
            
            if (motivoCancelacionId.HasValue && motivoCancelacionId.Value > 0)
            {
                pedido.MotivoCancelacionId = motivoCancelacionId.Value;
            }
            
            if (!string.IsNullOrEmpty(justificacion))
            {
                pedido.JustificacionCancelacion = justificacion;
            }

            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene un motivo de cancelación por su ID y valida que esté activo
        /// </summary>
        public async Task<MotivoCancelacion?> GetMotivoCancelacionAsync(int idMotivo)
        {
            return await _context.MotivosCancelacion
                .FirstOrDefaultAsync(m => m.Id == idMotivo && m.Activo);
        }

        /// <summary>
        /// Obtiene todos los motivos de cancelación activos
        /// </summary>
        public async Task<List<MotivoCancelacion>> GetMotivosCancelacionActivosAsync()
        {
            return await _context.MotivosCancelacion
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Valida si un pedido puede ser cancelado según su estado actual
        /// </summary>
        public async Task<bool> CanCancelOrderAsync(int idPedido)
        {
            var pedido = await _context.Pedidos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IDPedido == idPedido);

            if (pedido == null) return false;

            // No se pueden cancelar pedidos ya entregados o cancelados
            // Estados: 7 = Entregado, 9 = Cancelado automáticamente, 10 = Cancelado
            return pedido.IDEstadoDePedido != 7 && pedido.IDEstadoDePedido != 9 && pedido.IDEstadoDePedido != 10;
        }

        /// <summary>
        /// Obtiene información detallada del pedido para validación
        /// </summary>
        public async Task<(Pedido? Pedido, int IntentosEntrega)> GetOrderDetailForCancellationAsync(int idPedido)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.IDPedido == idPedido);

            if (pedido == null) return (null, 0);

            return (pedido, pedido.IntentosEntregaFallida);
        }
    }
}