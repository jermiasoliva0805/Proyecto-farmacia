using Back.Data;
using Back.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Back.Services
{
    public class PedidosDemoradosBackgroundService : BackgroundService
    {
        private static readonly TimeSpan IntervaloRevision = TimeSpan.FromMinutes(5);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PedidosDemoradosBackgroundService> _logger;

        public PedidosDemoradosBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PedidosDemoradosBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MarcarPedidosDemoradosAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al marcar pedidos demorados automáticamente.");
                }

                await Task.Delay(IntervaloRevision, stoppingToken);
            }
        }

        private async Task MarcarPedidosDemoradosAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var ahora = DateTime.Now;

            var pedidosDemorados = await context.Pedidos
                .Where(p => p.Estado != "Demorado")
                .Where(p => p.IDEstadoDePedido != 7 && p.IDEstadoDePedido != 9 && p.IDEstadoDePedido != 10)
                .Where(p => p.FechaEntregaEstimada != DateTime.MinValue)
                .Where(p => ahora > p.FechaEntregaEstimada)
                .ToListAsync(cancellationToken);

            if (!pedidosDemorados.Any())
                return;

            foreach (var pedido in pedidosDemorados)
            {
                pedido.IDEstadoDePedido = 3;
                pedido.EstadoActual = "Demorado";
                pedido.Estado = "Demorado";
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Pedidos demorados actualizados automáticamente: {Cantidad}", pedidosDemorados.Count);
        }
    }
}