using Back.Data;
using Back.DTOs;
using Back.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back.Repositories.Interfaces;

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
            // Iniciamos la base de la consulta con todas las relaciones necesarias
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EstadoDePedido)
                .Include(p => p.Usuario)
                .AsNoTracking()
                .AsQueryable();

            // 1. Filtrado por Búsqueda (ID o Nombre/Apellido de Cliente)
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                string term = filters.Search.ToLower();
                query = query.Where(p =>
                    p.IDPedido.ToString().Contains(term) ||
                    (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(term)) ||
                    (p.Cliente != null && p.Cliente.Apellido.ToLower().Contains(term))
                );
            }

            // 2. Filtrado por Estado
            if (filters.IDEstadoDePedido.HasValue && filters.IDEstadoDePedido.Value > 0)
            {
                query = query.Where(p => p.IDEstadoDePedido == filters.IDEstadoDePedido.Value);
            }

            // 3. Filtrado por Usuario (Responsable: Operario o Cadete)
            if (filters.IDUsuario.HasValue && filters.IDUsuario.Value > 0)
            {
                query = query.Where(p => p.IDUsuario == filters.IDUsuario.Value);
            }

            // 4. Filtrado por Cliente específico
            if (filters.IDCliente.HasValue && filters.IDCliente.Value > 0)
            {
                query = query.Where(p => p.IDCliente == filters.IDCliente.Value);
            }

            // 5. Filtrado por rango de fechas
            if (filters.FechaDesde.HasValue)
            {
                var desde = filters.FechaDesde.Value.Date;
                query = query.Where(p => p.Fecha.Date >= desde);
            }

            if (filters.FechaHasta.HasValue)
            {
                var hasta = filters.FechaHasta.Value.Date;
                query = query.Where(p => p.Fecha.Date <= hasta);
            }

            // 6. Proyección y ejecución de la consulta
            return await query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new OrderSummaryDTO
                {
                    IDPedido = p.IDPedido,
                    Fecha = p.Fecha,
                    Total = p.Total,
                    IDEstadoDePedido = p.IDEstadoDePedido,
                    EstadoNombre = p.EstadoDePedido != null ? p.EstadoDePedido.NombreEstado : "Sin Estado",
                    ClienteNombre = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Sin Cliente",
                    ResponsableNombre = p.Usuario != null ? p.Usuario.Nombre : "Sin Asignar",
                    FechaEntregaEstimada = p.FechaEntregaEstimada,
                    FechaEntregaReal = p.FechaEntregaReal
                })
                .ToListAsync();
        }

        public async Task<bool> ActualizarEstadoPedidoAsync(ChangeOrderStatusDTO datos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pedido = await _context.Pedidos.FindAsync(datos.IDPedido);
                if (pedido == null) return false;

                var ahora = DateTime.Now;

                // 1. "Cerrar" el estado anterior en el historial
                // Buscamos el registro del historial que todavía no tiene fecha de fin para este pedido
                var historialAnterior = await _context.HistorialesDeEstados
                    .Where(h => h.IDPedido == datos.IDPedido && h.fecha_hora_fin == null)
                    .FirstOrDefaultAsync();

                if (historialAnterior != null)
                {
                    historialAnterior.fecha_hora_fin = ahora;
                }

                // 2. Actualizar el pedido principal
                pedido.IDEstadoDePedido = datos.IDNuevoEstado;
                pedido.IDUsuario = datos.IDUsuario;

                if (datos.IDNuevoEstado == 7)
                {
                    pedido.FechaEntregaReal = ahora;
                }

                // Lógica de cancelación
                if (!string.IsNullOrEmpty(datos.MotivoCancelacion))
                {
                    if (int.TryParse(datos.MotivoCancelacion, out int idMotivo))
                    {
                        pedido.MotivoCancelacionId = idMotivo;
                    }
                    pedido.JustificacionCancelacion = datos.MotivoCancelacion;
                }
                else
                {
                    pedido.MotivoCancelacionId = null;
                    pedido.JustificacionCancelacion = null;
                }

                // 3. Crear el nuevo registro de historial (el nuevo estado actual)
                var nuevoHistorial = new HistorialDeEstados
                {
                    IDPedido = datos.IDPedido,
                    IDEstadoDePedido = datos.IDNuevoEstado,
                    IDUsuario = datos.IDUsuario,
                    fecha_hora_inicio = ahora,
                    fecha_hora_fin = null, // Queda abierto porque es el actual
                    Observaciones = datos.Observaciones ?? "Cambio de estado"
                };

                _context.HistorialesDeEstados.Add(nuevoHistorial);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<Pedido> GetByIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.EstadoDePedido)
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.IDPedido == id);
        }

        public async Task<bool> UpdateAsync(Pedido pedido)
        {
            _context.Entry(pedido).State = EntityState.Modified;
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}