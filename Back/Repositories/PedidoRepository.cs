using Back.Data;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _context;

        public PedidoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderSummaryDTO>> GetFilteredOrdersAsync(OrderFilterDTO filters)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EstadoDePedido)
                .Include(p => p.Usuario)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                string term = filters.Search.ToLower();
                query = query.Where(p =>
                    p.IDPedido.ToString().Contains(term) ||
                    (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(term)) ||
                    (p.Cliente != null && p.Cliente.Apellido.ToLower().Contains(term))
                );
            }

            if (filters.IDEstadoDePedido.HasValue && filters.IDEstadoDePedido.Value > 0)
                query = query.Where(p => p.IDEstadoDePedido == filters.IDEstadoDePedido.Value);

            return await query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new OrderSummaryDTO
                {
                    IDPedido = p.IDPedido,
                    Fecha = p.Fecha,
                    Total = p.Total,
                    IDEstadoDePedido = p.IDEstadoDePedido,
                    EstadoNombre = p.EstadoDePedido != null ? p.EstadoDePedido.NombreEstado : "Sin Estado",
                    ClienteNombre = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Sin Cliente"
                })
                .ToListAsync();
        }

        public async Task<Pedido?> GetByIdAsync(int idPedido)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Barrio)
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Localidad)
                .Include(p => p.Sucursal)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IDPedido == idPedido);
        }

        public async Task UpdateAsync(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }

   public async Task<bool> ActualizarEstadoPedidoAsync(ChangeOrderStatusDTO dto)
    {
        var pedido = await _context.Pedidos
            .FirstOrDefaultAsync(p => p.IDPedido == dto.IDPedido);

        if (pedido == null) return false;

        int estadoFinal = pedido.IDEstadoDePedido; // Estado que se grabará

        // --- LÓGICA DE INTENTOS FALLIDOS ---
        if (dto.IDNuevoEstado == 8) // Entrega Fallida
        {
            pedido.IntentosEntregaFallida++;

            if (pedido.IntentosEntregaFallida >= 3)
            {
                // Cancelar automáticamente
                estadoFinal = 9; // Cancelado
                pedido.IDEstadoDePedido = 9;
                pedido.EstadoActual = "Cancelado";
                pedido.JustificacionCancelacion = "Cancelación automática: superó los 3 intentos fallidos.";
            }
            else
            {
                // Mantener en fallido
                estadoFinal = 8;
                pedido.IDEstadoDePedido = 8;
                pedido.EstadoActual = "Entrega fallida";
            }
        }
        else if (dto.IDNuevoEstado == 9) // Cancelación manual
        {
            estadoFinal = 9;
            pedido.IDEstadoDePedido = 9;
            pedido.EstadoActual = "Cancelado";
        }
        else
        {
            estadoFinal = dto.IDNuevoEstado;
            pedido.IDEstadoDePedido = dto.IDNuevoEstado;

            if (dto.IDNuevoEstado == 7) // Entregado
                pedido.IntentosEntregaFallida = 0;
        }

        // --- GRABAR HISTORIAL CON EL ESTADO CORRECTO ---
        var nuevoHistorial = new HistorialDeEstados
        {
            IDPedido = pedido.IDPedido,
            IDEstadoDePedido = estadoFinal, // ← USA LA VARIABLE CALCULADA
            IDUsuario = dto.IDUsuario,
            fecha_hora_inicio = DateTime.Now,
            Observaciones = dto.IDNuevoEstado == 8
                ? $"Intento #{pedido.IntentosEntregaFallida} fallido. Motivo: {dto.Observaciones}"
                : (dto.IDNuevoEstado == 9 ? "Pedido cancelado" : dto.Observaciones)
        };

        _context.HistorialesDeEstados.Add(nuevoHistorial);
        _context.Pedidos.Update(pedido);

        await _context.SaveChangesAsync();
        return true;
    }
    }
}