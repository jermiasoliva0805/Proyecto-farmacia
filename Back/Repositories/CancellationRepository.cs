using Back.Models;
using Back.Interfaces;
using Back.Data;

namespace Back.Repositories
{
    public class CancellationRepository : ICancellationRepository
    {
        private readonly AppDbContext _context;

        public CancellationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CancelOrderAsync(int idPedido, string motivo, string idUsuario)
        {
            var pedido = await _context.Pedidos.FindAsync(idPedido); 
            if (pedido != null)
            {
                // Según tu modelo EstadoDePedido, 9 suele ser cancelado
                pedido.IDEstadoDePedido = 9;
                pedido.EstadoActual = "Cancelado"; // Actualizamos el string también

                await _context.SaveChangesAsync();
            }
        }
    }
}