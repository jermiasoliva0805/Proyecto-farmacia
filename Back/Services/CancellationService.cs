using Back.Interfaces;
using Back.Services.Interfaces;
using System.Threading.Tasks;

namespace Back.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly ICancellationRepository _cancellationRepo;
        private readonly IHistoryRepository _historyRepo; // Inyectamos historial para dejar rastro

        public CancellationService(ICancellationRepository cancellationRepo, IHistoryRepository historyRepo)
        {
            _cancellationRepo = cancellationRepo;
            _historyRepo = historyRepo;
        }

        public async Task CancelarPedido(int idPedido, string motivo, int idUsuario)
        {
            // 1. Ejecutamos la cancelación en el repo de pedidos
            await _cancellationRepo.CancelOrderAsync(idPedido, motivo, idUsuario.ToString());

            // 2. (Opcional pero recomendado) Dejamos registro en el historial de que fue cancelado
            // Usamos los modelos reales que me pasaste antes
            var historialCancelacion = new Back.Models.HistorialDeEstados
            {
                IDPedido = idPedido,
                IDEstadoDePedido = 9, // ID que representa "Cancelado"
                fecha_hora_inicio = System.DateTime.Now,
                Observaciones = $"Cancelado por el usuario. Motivo: {motivo}",
                IDUsuario = idUsuario
            };

            await _historyRepo.AddAsync(historialCancelacion);
        }
    }
}