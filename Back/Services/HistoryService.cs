using Back.Models;
using Back.Interfaces;
using Back.Services.Interfaces;
using Back.DTOs;

namespace Back.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _historyRepo;

        public HistoryService(IHistoryRepository historyRepo)
        {
            _historyRepo = historyRepo;
        }

        public async Task RegistrarCambioEstado(OrderHistoryDTO dto)
        {
            var nuevoHistorial = new HistorialDeEstados
            {
                IDPedido = dto.PedidoId,
                IDEstadoDePedido = 1, // Aquí iría la lógica del ID según el nombre del estado
                fecha_hora_inicio = DateTime.Now,
                Observaciones = dto.Motivo,
                IDUsuario = 1 // Esto debería venir del usuario logueado
            };

            await _historyRepo.AddAsync(nuevoHistorial);
        }

        public async Task<IEnumerable<OrderHistoryDTO>> ObtenerHistorialPorPedido(int idPedido)
        {
            var historiales = await _historyRepo.GetByPedidoIdAsync(idPedido);
            // Aquí mapeamos de Model a DTO (esto se puede hacer con AutoMapper después)
            return historiales.Select(h => new OrderHistoryDTO
            {
                PedidoId = h.IDPedido,
                FechaCambio = h.fecha_hora_inicio,
                Motivo = h.Observaciones
            });
        }
    }
}