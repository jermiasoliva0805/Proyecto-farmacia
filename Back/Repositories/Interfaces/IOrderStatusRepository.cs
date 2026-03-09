using Back.Models;

namespace Back.Repositories.Interfaces
{
    public interface IOrderStatusRepository
    {
        Task<bool> ActualizarEstadoAsync(HistorialDeEstados nuevoHistorial, Pedido pedidoActualizado = null);
    }
}
