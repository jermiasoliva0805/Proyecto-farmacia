using Back.Data;
using Back.Models;
using Back.DTOs;
using Back.Repositories.Interfaces;
using Back.Utils;
using Microsoft.EntityFrameworkCore;
 
namespace Back.Repositories
{
    public class OrderRepository : GenericRepository<Pedido>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }
 
        public async Task<IEnumerable<OrderSummaryDTO>> GetOrdersByStatusAsync(int statusId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.IDEstadoDePedido == statusId)
                .Select(p => new OrderSummaryDTO
                {
                    IDPedido = p.IDPedido,
                    Fecha = p.Fecha,
                    Total = p.Total,
                    IDEstadoDePedido = p.IDEstadoDePedido,
                    EstadoNombre = p.Estado == "Demorado"
                        ? "Demorado"
                        : p.EstadoActual,
                    ClienteNombre = p.Cliente != null ? p.Cliente.Nombre : "Consumidor Final",
                    ResponsableNombre = p.Usuario != null ? p.Usuario.Nombre : "Sin asignar"
                })
                .ToListAsync();
        }
 
        public async Task<int> CreateOrderAsync(Pedido pedido, int idUsuario)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                pedido.Fecha = DateTime.Now;
                pedido.IDEstadoDePedido = 1; // "Sin preparar"
                pedido.EstadoActual = "Sin preparar";
 
                // ✅ Calcular fecha de entrega estimada: 48 horas hábiles (lunes a viernes)
                pedido.FechaEntregaEstimada = DateTimeHelper.CalcularFechaEntregaEstimada(pedido.Fecha);
 
                if (pedido.Detalles != null && pedido.Detalles.Any())
                {
                    pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                }
 
                // ✅ FIX: Solo usar zona del barrio del cliente como fallback
                // si el frontend NO envió una zona
                if (!pedido.ZonaId.HasValue && pedido.IDCliente > 0)
                {
                    var cliente = await _context.Clientes
                        .Include(c => c.Barrio)
                        .FirstOrDefaultAsync(c => c.IDCliente == pedido.IDCliente);
 
                    if (cliente?.Barrio != null && cliente.Barrio.ZonaId.HasValue)
                    {
                        pedido.ZonaId = cliente.Barrio.ZonaId;
                        Console.WriteLine($"[OrderRepository] ZonaId asignada desde barrio del cliente: {pedido.ZonaId}");
                    }
                }
                else
                {
                    Console.WriteLine($"[OrderRepository] ZonaId recibida desde el frontend: {pedido.ZonaId}");
                }
 
                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();
 
                // ✅ Crear historial DESPUÉS de que el pedido está guardado (así tiene ID)
                var historialInicial = new HistorialDeEstados
                {
                    IDPedido = pedido.IDPedido,
                    IDEstadoDePedido = 1, // "Sin preparar"
                    IDUsuario = idUsuario,
                    fecha_hora_inicio = DateTime.UtcNow,
                    Observaciones = "Pedido recibido e ingresado al sistema."
                };
 
                _context.HistorialesDeEstados.Add(historialInicial);
                await _context.SaveChangesAsync();
 
                await transaction.CommitAsync();
                return pedido.IDPedido;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[OrderRepository] Error al crear pedido: {ex.Message}");
                throw;
            }
        }
 
        // Incluye Cliente para poder acceder al mail
        public async Task<Pedido> GetByIdWithClienteAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.IDPedido == id);
        }
 
        public async Task<Pedido> GetOrderWithDetailsAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IDPedido == id);
        }
 
        // Obtener todos los pedidos de un cliente con sus detalles
        public async Task<IEnumerable<Pedido>> GetClientOrdersAsync(int clientId)
        {
            return await _context.Pedidos
                .Where(p => p.IDCliente == clientId)
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .ToListAsync();
        }
 
        // Incluye Cliente y Detalles para poder acceder al mail y productos
        public async Task<Pedido> GetByIdWithClienteAndDetailsAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IDPedido == id);
        }
    }
}