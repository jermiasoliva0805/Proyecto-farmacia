using Back.Data;
using Back.Hubs;
using Back.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
 
namespace Back.Services
{
    public class PedidosDemoradosBackgroundService : BackgroundService
    {
        // Intervalo de revisión: cada 5 minutos
        private static readonly TimeSpan IntervaloRevision = TimeSpan.FromMinutes(5);
 
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PedidosDemoradosBackgroundService> _logger;
        private readonly IHubContext<PedidosHub> _hubContext;
 
        public PedidosDemoradosBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PedidosDemoradosBackgroundService> logger,
            IHubContext<PedidosHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
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
 
            // Buscar pedidos que:
            // 1. Aún NO están marcados como demorados (para no re-procesar)
            // 2. No están en estados finales (Entregado=7, Cancelado=9, EntregaFallida=10 si aplica)
            // 3. Superaron su FechaEntregaEstimada
            var pedidosNuevamenteDemorados = await context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.EstadoDePedido)
                .Where(p => !p.EsDemorado)
                .Where(p => p.IDEstadoDePedido != 7   // No Entregado
                         && p.IDEstadoDePedido != 9   // No Cancelado
                         && p.IDEstadoDePedido != 10) // No estado extra final
                .Where(p => p.FechaEntregaEstimada != DateTime.MinValue)
                .Where(p => ahora > p.FechaEntregaEstimada)
                .ToListAsync(cancellationToken);
 
            if (!pedidosNuevamenteDemorados.Any())
                return;
 
            var notificaciones = new List<PedidoDemoradoNotificacion>();
 
            foreach (var pedido in pedidosNuevamenteDemorados)
            {
                // ── CAMBIO CLAVE: solo se setea el flag, NO se modifica el estado principal ──
                pedido.EsDemorado = true;
                pedido.FechaMarcadoDemorado = ahora;
                // pedido.Estado y pedido.IDEstadoDePedido quedan INTACTOS
                // ─────────────────────────────────────────────────────────────────────────────
 
                notificaciones.Add(new PedidoDemoradoNotificacion
                {
                    IDPedido = pedido.IDPedido,
                    EstadoPrincipal = pedido.EstadoDePedido?.NombreEstado ?? pedido.Estado,
                    ResponsableNombre = pedido.Usuario != null
                        ? $"{pedido.Usuario.Nombre} {pedido.Usuario.Apellido}"
                        : "Sin asignar",
                    ResponsableRol = pedido.Usuario?.Rol ?? string.Empty,
                    FechaMarcado = ahora
                });
            }
 
            await context.SaveChangesAsync(cancellationToken);
 
            _logger.LogInformation(
                "Pedidos marcados como demorados automáticamente: {Cantidad}",
                pedidosNuevamenteDemorados.Count);
 
            // ── Push en tiempo real a todos los clientes conectados vía SignalR ──
            // Se envía la lista completa de pedidos recién marcados.
            // El cliente decide cómo mostrarlos (campana, toast, etc.)
            await _hubContext.Clients.All.SendAsync(
                "PedidosDemoradosActualizados",
                notificaciones,
                cancellationToken);
        }
    }
}