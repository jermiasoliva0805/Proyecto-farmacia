using Microsoft.AspNetCore.SignalR;
 
namespace Back.Hubs
{
    /// <summary>
    /// Hub de SignalR para notificaciones en tiempo real.
    /// Los clientes se conectan y quedan suscritos a eventos de pedidos demorados.
    /// </summary>
    public class PedidosHub : Hub
    {
        // El servidor puede llamar desde cualquier servicio a:
        //   _hubContext.Clients.All.SendAsync("PedidoDemorado", notificacion)
        // y todos los clientes conectados lo recibirán.
 
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
 
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
 
    /// <summary>
    /// DTO que se envía al cliente por SignalR cuando un pedido pasa a demorado.
    /// </summary>
    public class PedidoDemoradoNotificacion
    {
        public int IDPedido { get; set; }
        public string EstadoPrincipal { get; set; } = string.Empty;
        public string ResponsableNombre { get; set; } = string.Empty;
        public string ResponsableRol { get; set; } = string.Empty;
        public DateTime FechaMarcado { get; set; }
    }
}