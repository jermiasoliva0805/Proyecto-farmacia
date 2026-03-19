using Back.Data;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Proyecto_farmacia.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly AppDbContext _context;

        public ReporteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int? idSucursal = null)
        {
            var hasta = fechaHasta.AddDays(1).AddSeconds(-1);

            var query = _context.Pedidos
                .Include(p => p.Usuario)
                .Where(p => p.Usuario.Rol == "Cadete") // Filtrar solo cadetes
                .Where(p => p.Fecha >= fechaDesde && p.Fecha <= hasta)
                .AsQueryable();

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var pedidos = await query.ToListAsync();

            var reporte = pedidos
                .GroupBy(p => new { p.IDUsuario, p.Usuario.Nombre })
                .Select(g => new EntregaPorCadeteDTO
                {
                    IDCadete = g.Key.IDUsuario,
                    NombreCadete = g.Key.Nombre,
                    TotalPedidosAsignados = g.Count(),
                    EntregasExitosas = g.Count(p => p.IDEstadoDePedido == 7),
                    EntregasFallidas = g.Count(p => p.IDEstadoDePedido == 9),
                    TotalRecaudado = g.Sum(p => p.Total),
                    PorcentajeEfectividad = g.Count() > 0 ? (g.Count(p => p.IDEstadoDePedido == 7) * 100.0 / g.Count()) : 0
                })
                .OrderByDescending(r => r.EntregasExitosas)
                .ToList();

            return reporte;
        }

        public async Task<List<RankingClienteDTO>> GetRankingClientesFrecuentesAsync(int dias = 7, int? idSucursal = null)
        {
            var fechaDesde = DateTime.Now.AddDays(-dias);

            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Fecha >= fechaDesde)
                .AsQueryable();

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var pedidos = await query.ToListAsync();

            var ranking = pedidos
                .GroupBy(p => new { p.IDCliente, p.Cliente.Nombre })
                .Select(g => new RankingClienteDTO
                {
                    NombreCliente = g.Key.Nombre,
                    CantidadPedidos = g.Count(),
                    GastoTotal = g.Sum(p => p.Total),
                    TicketPromedio = g.Count() > 0 ? g.Sum(p => p.Total) / g.Count() : 0,
                    UltimaCompra = g.Max(p => p.Fecha)
                })
                .OrderByDescending(r => r.CantidadPedidos)
                .Take(10)
                .ToList();

            return ranking;
        }

        public async Task<List<ClienteFacturacionDTO>> GetRankingClientesFacturacionAsync(int dias = 7, int? idSucursal = null)
        {
            var fechaDesde = DateTime.Now.AddDays(-dias);

            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Fecha >= fechaDesde)
                .AsQueryable();

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var pedidos = await query.ToListAsync();

            var ranking = pedidos
                .GroupBy(p => new { p.IDCliente, p.Cliente.Nombre })
                .Select(g => new ClienteFacturacionDTO
                {
                    NombreCliente = g.Key.Nombre,
                    TotalFacturado = g.Sum(p => p.Total),
                    CantidadPedidos = g.Count()
                })
                .OrderByDescending(c => c.TotalFacturado)
                .Take(10)
                .ToList();

            return ranking;
        }

        public async Task<ReportePedidosCanceladosDTO> GetReportePedidosCanceladosAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null)
        {
            const int ID_ESTADO_CANCELADO = 9;

            // Establecer fechas por defecto si no se proporcionan
            var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
            var hasta = fechaHasta ?? DateTime.Now;

            // Asegurar que 'hasta' incluya todo el día (23:59:59)
            hasta = hasta.AddDays(1).AddSeconds(-1);

            // Obtener todos los pedidos creados en el rango
            var queryTodosPedidos = _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

            // Obtener pedidos cancelados AUTOMÁTICAMENTE (3 intentos fallidos - NO tienen motivo)
            var queryCancelados = _context.Pedidos
                .Where(p => p.IDEstadoDePedido == ID_ESTADO_CANCELADO)
                .Where(p => p.MotivoCancelacionId == null)  // Sin motivo = auto-cancelado
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

            // Aplicar filtro de sucursal si es necesario
            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                queryTodosPedidos = queryTodosPedidos.Where(p => p.IDSucursal == idSucursal.Value);
                queryCancelados = queryCancelados.Where(p => p.IDSucursal == idSucursal.Value);
            }

            // Contar totales
            var totalPedidos = await queryTodosPedidos.CountAsync();
            var pedidosCancelados = await queryCancelados.ToListAsync();
            var totalCancelados = pedidosCancelados.Count;

            // Calcular porcentaje
            var porcentaje = totalPedidos > 0 ? (totalCancelados * 100.0m / totalPedidos) : 0m;

            // Suma del monto total cancelado
            var montoTotal = pedidosCancelados.Sum(p => p.Total);

            // Reporte simple sin motivos (estos no tienen motivos)
            var detallesPorMotivo = new List<DetalleCancelacionDTO>();

            return new ReportePedidosCanceladosDTO
            {
                TotalPedidosCancelados = totalCancelados,
                PorcentajeDelTotal = porcentaje,
                MontoTotalCancelado = montoTotal,
                DetallePorMotivo = detallesPorMotivo
            };
        }

        public async Task<ReporteCancelacionesPorMotivoDTO> GetReporteCancelacionesPorMotivoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null)
        {
            const int ID_ESTADO_CANCELADO = 9;

            // Establecer fechas por defecto
            var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
            var hasta = fechaHasta ?? DateTime.Now;

            // Asegurar que 'hasta' incluya todo el día (23:59:59)
            hasta = hasta.AddDays(1).AddSeconds(-1);

            // Obtener todos los pedidos creados en el rango
            var queryTodosPedidos = _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

            // Obtener pedidos cancelados MANUALMENTE por el admin (tienen motivo explícito asignado)
            var queryCancelados = _context.Pedidos
                .Include(p => p.MotivoCancelacion)
                .Where(p => p.IDEstadoDePedido == ID_ESTADO_CANCELADO)
                .Where(p => p.MotivoCancelacionId != null)  // Solo cancelaciones manuales
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

            // Aplicar filtro de sucursal
            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                queryTodosPedidos = queryTodosPedidos.Where(p => p.IDSucursal == idSucursal.Value);
                queryCancelados = queryCancelados.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var totalPedidos = await queryTodosPedidos.CountAsync();
            var pedidosCancelados = await queryCancelados.ToListAsync();
            var totalCancelados = pedidosCancelados.Count;

            var porcentajeCancelacion = totalPedidos > 0 ? (totalCancelados * 100.0m / totalPedidos) : 0m;
            var ingresosPerdidos = pedidosCancelados.Sum(p => p.Total);

            // Obtener principal motivo
            var principalMotivo = pedidosCancelados
                .GroupBy(p => p.MotivoCancelacion!.Nombre)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A";

            // Detalles por motivo
            var detalleMotivos = pedidosCancelados
                .GroupBy(p => p.MotivoCancelacion!.Nombre)
                .Select(g => new CancelacionPorMotivoDTO
                {
                    Motivo = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = totalCancelados > 0 ? (g.Count() * 100.0m / totalCancelados) : 0m,
                    MontoPerdido = g.Sum(p => p.Total)
                })
                .OrderByDescending(d => d.Cantidad)
                .ToList();

            return new ReporteCancelacionesPorMotivoDTO
            {
                TotalPedidos = totalPedidos,
                TotalCancelados = totalCancelados,
                PorcentajeCancelacion = porcentajeCancelacion,
                IngresosPerdidos = ingresosPerdidos,
                PrincipalMotivo = principalMotivo,
                DetalleMotivos = detalleMotivos
            };
        }

        public async Task<List<TopProductosDTO>> GetTop10ProductosMasVendidosAsync(int dias = 7, int? idSucursal = null)
        {
            // Regla de Negocio: Contar todos los productos de pedidos NO cancelados
            const int ID_ESTADO_CANCELADO = 9;
            
            // Calcular la fecha desde la cual filtrar
            DateTime fechaDesde = DateTime.Now.AddDays(-dias);

            var query = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .Where(p => p.IDEstadoDePedido != ID_ESTADO_CANCELADO) // Excluir solo cancelados
                .Where(p => p.Fecha >= fechaDesde); // Filtro de fecha

            // Agregar filtro de sucursal si viene especificado
            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

            // Obtener todos los pedidos relevantes
            var pedidos = await query.ToListAsync();

            // Procesar: agrupar detalles por producto e calcular estadísticas
            var totalUnidades = pedidos
                .SelectMany(p => p.Detalles)
                .Sum(d => d.Cantidad);

            var topProductos = pedidos
                .SelectMany(p => p.Detalles)
                .GroupBy(d => new { d.IDProducto, d.Producto.NombreProducto })
                .Select(g => new TopProductosDTO
                {
                    IDProducto = g.Key.IDProducto,
                    NombreProducto = g.Key.NombreProducto,
                    UnidadesVendidas = g.Sum(d => d.Cantidad),
                    Porcentaje = totalUnidades > 0 ? (g.Sum(d => d.Cantidad) * 100m) / totalUnidades : 0,
                    PrecioPromedio = g.Count() > 0 ? g.Average(d => d.PrecioUnitario) : 0
                })
                .OrderByDescending(p => p.UnidadesVendidas)
                .Take(10)
                .ToList();

            return topProductos;
        }

        public async Task<TiemposProcesoDTO> GetReporteTiemposProcesoAsync(int dias = 7, int? idSucursal = null, int? idEstado = null)
        {
            // Calcular la fecha desde la cual filtrar
            DateTime fechaDesde = DateTime.Now.AddDays(-dias);

            // Obtener todos los pedidos en el rango con su historial de estados y relaciones
            var query = _context.Pedidos
                .Include(p => p.HistorialDeEstados)
                    .ThenInclude(h => h.EstadoDePedido)
                .Where(p => p.Fecha >= fechaDesde);

            // Agregar filtro de sucursal si viene especificado
            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

            // Agregar filtro de estado si viene especificado
            if (idEstado.HasValue && idEstado.Value > 0)
            {
                Console.WriteLine($"[REPO DEBUG] Aplicando filtro de estado: {idEstado.Value}");
                query = query.Where(p => p.IDEstadoDePedido == idEstado.Value);
            }

            var pedidos = await query.ToListAsync();

            Console.WriteLine($"[REPO DEBUG] TiemposProceso - Total pedidos cargados: {pedidos.Count}");

            // Calcular tiempos por fase para cada pedido
            var detalles = new List<DetalleTiempoProcesoDTO>();
            var tiemposPorFase = new Dictionary<string, List<double>>
            {
                { "Espera", new List<double>() },
                { "Preparación", new List<double>() },
                { "Despacho", new List<double>() },
                { "Viaje", new List<double>() }
            };

            foreach (var pedido in pedidos)
            {
                var historial = pedido.HistorialDeEstados == null 
                    ? new List<HistorialDeEstados>() 
                    : pedido.HistorialDeEstados.OrderBy(h => h.fecha_hora_inicio).ToList();

                Console.WriteLine($"[REPO DEBUG] Pedido #{pedido.IDPedido} (Estado actual: {pedido.IDEstadoDePedido}) - Historial entries: {historial.Count}");
                
                if (historial.Count > 0)
                {
                    foreach (var h in historial)
                    {
                        Console.WriteLine($"  → Estado ID: {h.IDEstadoDePedido}, Nombre: {h.EstadoDePedido?.NombreEstado ?? "NULL"}, Fecha: {h.fecha_hora_inicio:yyyy-MM-dd HH:mm:ss}");
                    }
                }
                else
                {
                    Console.WriteLine($"  ⚠️ HISTORIAL VACÍO para pedido #{pedido.IDPedido}");
                }

                // Encontrar transiciones entre estados
                var estado1 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 1);
                var estado2 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 2);
                var estado4 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 4);
                var estado6 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 6);
                var estado7 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 7);

                // Calcular tiempos de cada fase
                double espera = 0;
                double preparacion = 0;
                double despacho = 0;
                double viaje = 0;

                if (estado1 != null && estado2 != null)
                {
                    espera = (estado2.fecha_hora_inicio - estado1.fecha_hora_inicio).TotalMinutes;
                    tiemposPorFase["Espera"].Add(espera);
                    Console.WriteLine($"  ✓ Espera: {espera:F2} min");
                }

                if (estado2 != null && estado4 != null)
                {
                    preparacion = (estado4.fecha_hora_inicio - estado2.fecha_hora_inicio).TotalMinutes;
                    tiemposPorFase["Preparación"].Add(preparacion);
                    Console.WriteLine($"  ✓ Preparación: {preparacion:F2} min");
                }

                if (estado4 != null && estado6 != null)
                {
                    despacho = (estado6.fecha_hora_inicio - estado4.fecha_hora_inicio).TotalMinutes;
                    tiemposPorFase["Despacho"].Add(despacho);
                    Console.WriteLine($"  ✓ Despacho: {despacho:F2} min");
                }

                if (estado6 != null && estado7 != null)
                {
                    viaje = (estado7.fecha_hora_inicio - estado6.fecha_hora_inicio).TotalMinutes;
                    tiemposPorFase["Viaje"].Add(viaje);
                    Console.WriteLine($"  ✓ Viaje: {viaje:F2} min");
                }

                // Determinar el estado final
                var estadoFinal = historial.Count > 0 
                    ? (historial.Last().EstadoDePedido?.NombreEstado ?? "Desconocido") 
                    : "Sin historial";

                // Agregar a detalles
                detalles.Add(new DetalleTiempoProcesoDTO
                {
                    IdPedido = pedido.IDPedido,
                    Espera = espera,
                    Preparacion = preparacion,
                    Despacho = despacho,
                    Viaje = viaje,
                    EstadoFinal = estadoFinal,
                    EsAlertaDespacho = despacho > 60 // Alerta si despacho > 60 minutos
                });
            }

            // Calcular promedios por fase
            var fases = new List<FaseProcesoDTO>
            {
                new FaseProcesoDTO
                {
                    Nombre = "Espera",
                    TiempoPromedio = tiemposPorFase["Espera"].Count > 0 ? tiemposPorFase["Espera"].Average() : 0,
                    Color = "#6366f1" // Indigo
                },
                new FaseProcesoDTO
                {
                    Nombre = "Preparación",
                    TiempoPromedio = tiemposPorFase["Preparación"].Count > 0 ? tiemposPorFase["Preparación"].Average() : 0,
                    Color = "#8b5cf6" // Violet
                },
                new FaseProcesoDTO
                {
                    Nombre = "Despacho",
                    TiempoPromedio = tiemposPorFase["Despacho"].Count > 0 ? tiemposPorFase["Despacho"].Average() : 0,
                    Color = "#a78bfa" // Lilac
                },
                new FaseProcesoDTO
                {
                    Nombre = "Viaje",
                    TiempoPromedio = tiemposPorFase["Viaje"].Count > 0 ? tiemposPorFase["Viaje"].Average() : 0,
                    Color = "#c4b5fd" // Light Violet
                }
            };

            // Determinar punto crítico (la fase con mayor tiempo promedio)
            var puntoCritico = fases.OrderByDescending(f => f.TiempoPromedio).First();

            // Calcular eficiencia de despacho (% de pedidos con despacho < 30 min)
            var eficienciaDespacho = detalles.Count > 0
                ? (int)Math.Round((detalles.Count(d => d.Despacho < 30 && d.Despacho > 0) * 100.0) / detalles.Count)
                : 0;

            Console.WriteLine($"[REPO DEBUG] Resumen - Total: {pedidos.Count}, Detalles: {detalles.Count}, Punto Crítico: {puntoCritico.Nombre}");

            return new TiemposProcesoDTO
            {
                Fases = fases,
                PuntoCritico = puntoCritico.Nombre,
                TiempoPuntoCritico = puntoCritico.TiempoPromedio,
                EficienciaDespacho = eficienciaDespacho,
                TotalPedidos = pedidos.Count,
                Detalles = detalles.OrderByDescending(d => d.Despacho).ToList()
            };
        }
    }
}
