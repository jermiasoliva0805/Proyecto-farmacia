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
    // 1. Buscamos el pedido
    var pedido = await _context.Pedidos
        .FirstOrDefaultAsync(p => p.IDPedido == dto.IDPedido);

    if (pedido == null) return false;

    // --- LÓGICA DE INTENTOS FALLIDOS ---
    // Si el nuevo estado es 8 (Entrega Fallida)
    if (dto.IDNuevoEstado == 8) 
    {
        pedido.IntentosEntregaFallida++; 

        if (pedido.IntentosEntregaFallida >= 3)
        {
            pedido.IDEstadoDePedido = 9; // Cancelado
            pedido.EstadoActual = "Cancelado";
            pedido.JustificacionCancelacion = "Cancelación automática: superó los 3 intentos fallidos.";
        }
        else
        {
            pedido.IDEstadoDePedido = 8;
            pedido.EstadoActual = "Entrega fallida";
        }

        // --- MÉTODO PARA EL REPORTE RF6.4 ---
        public async Task<List<ReporteOperarioDTO>> GetTiempoPromedioArmadoAsync()
        {
            // Buscamos los estados 2 (Inicio) y 4 (Fin) en el historial
            var historiales = await _context.HistorialDeEstados
                .Include(h => h.Usuario)
                .Where(h => h.IDEstadoDePedido == 2 || h.IDEstadoDePedido == 4)
                .ToListAsync();

            // Calculamos la diferencia por cada pedido
            var tiemposPorPedido = historiales
                .GroupBy(h => h.IDPedido)
                .Select(grupo => {
                    var inicio = grupo.FirstOrDefault(h => h.IDEstadoDePedido == 2);
                    var fin = grupo.FirstOrDefault(h => h.IDEstadoDePedido == 4);

                    if (inicio != null && fin != null)
                    {
                        return new {
                            Nombre = inicio.Usuario != null ? inicio.Usuario.Nombre : "Operario Desconocido",
                            Minutos = (fin.fecha_hora_inicio - inicio.fecha_hora_inicio).TotalMinutes
                        };
                    }
                    return null;
                })
                .Where(x => x != null)
                .ToList();

            // Agrupamos por Operario para el promedio final
            return tiemposPorPedido
                .GroupBy(x => x!.Nombre)
                .Select(g => new ReporteOperarioDTO
                {
                    NombreOperario = g.Key,
                    TotalPedidosArmados = g.Count(),
                    TiempoPromedioMinutos = Math.Round(g.Average(x => x!.Minutos), 2)
                })
                .ToList();
        }
    }
    else 
    {
        pedido.IDEstadoDePedido = dto.IDNuevoEstado;
        // Si el estado es positivo (ej: Entregado), podrías resetear el contador si quisieras
        if (dto.IDNuevoEstado == 7) pedido.IntentosEntregaFallida = 0;
    }

    // --- GRABAR EN HISTORIAL (Usando tus nombres de campos) ---
    var nuevoHistorial = new HistorialDeEstados
    {
        IDPedido = pedido.IDPedido,
        IDEstadoDePedido = pedido.IDEstadoDePedido,
        IDUsuario = dto.IDUsuario,
        fecha_hora_inicio = DateTime.Now, // Tu campo se llama así
        Observaciones = dto.IDNuevoEstado == 8 
            ? $"Intento #{pedido.IntentosEntregaFallida} fallido. Motivo: {dto.Observaciones}" 
            : dto.Observaciones
    };

    _context.HistorialesDeEstados.Add(nuevoHistorial); // Tu DbSet se llama HistorialesDeEstados
    
    await _context.SaveChangesAsync();
    return true;
}
    }
}