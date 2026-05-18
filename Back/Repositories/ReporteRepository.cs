using Back.Data;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Back.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Proyecto_farmacia.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
 
namespace Back.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
 
        public ReporteRepository(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
 
        public async Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int idSucursal)
        {
            var hasta = fechaHasta.AddDays(1).AddSeconds(-1);
 
            // Obtener todos los cadetes activos en esta sucursal
            var cadetes = await _context.Usuarios
                .Where(u => u.Rol == "Cadete" && u.IDSucursal == idSucursal && !u.IsDeleted)
                .Select(u => new { u.IDUsuario, u.Nombre })
                .ToListAsync();
 
            // Si no hay cadetes, devolver lista vacía
            if (!cadetes.Any())
                return new List<EntregaPorCadeteDTO>();
 
            var idsCADETES = cadetes.Select(c => c.IDUsuario).ToList();
 
            // Obtener pedidos de esos cadetes en el rango de fechas
            var pedidos = await _context.Pedidos
                .Where(p => idsCADETES.Contains(p.IDUsuario))
                .Where(p => p.Fecha >= fechaDesde && p.Fecha <= hasta)
                .ToListAsync();
 
            // Agrupar por cadete
            var reportePorCadete = cadetes.Select(cadete => 
            {
                var pedidosCadete = pedidos.Where(p => p.IDUsuario == cadete.IDUsuario).ToList();
                
                return new EntregaPorCadeteDTO
                {
                    IDCadete              = cadete.IDUsuario,
                    NombreCadete          = cadete.Nombre,
                    TotalPedidosAsignados = pedidosCadete.Count,
                    EntregasExitosas      = pedidosCadete.Count(p => p.IDEstadoDePedido == 7),
                    EntregasFallidas      = pedidosCadete.Count(p => p.IDEstadoDePedido == 9),
                    TotalRecaudado        = pedidosCadete.Sum(p => p.Total),
                    PorcentajeEfectividad = pedidosCadete.Count > 0 
                        ? (pedidosCadete.Count(p => p.IDEstadoDePedido == 7) * 100.0 / pedidosCadete.Count)
                        : 0
                };
            })
            .OrderByDescending(r => r.EntregasExitosas)
            .ToList();
 
            return reportePorCadete;
        }
 
        public async Task<List<RankingClienteDTO>> GetRankingClientesFrecuentesAsync(int dias = 7)
        {
            int idSucursal = 1;
            var fechaDesde = DateTimeHelper.GetArgentinaTime().AddDays(-dias);
 
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Fecha >= fechaDesde)
                .Where(p => p.IDSucursal == idSucursal)
                .ToListAsync();
 
            return pedidos
                .GroupBy(p => new { p.IDCliente, p.Cliente.Nombre })
                .Select(g => new RankingClienteDTO
                {
                    NombreCliente   = g.Key.Nombre,
                    CantidadPedidos = g.Count(),
                    GastoTotal      = g.Sum(p => p.Total),
                    TicketPromedio  = g.Count() > 0 ? g.Sum(p => p.Total) / g.Count() : 0,
                    UltimaCompra    = g.Max(p => p.Fecha)
                })
                .OrderByDescending(r => r.CantidadPedidos)
                .Take(10)
                .ToList();
        }
 
        public async Task<List<ClienteFacturacionDTO>> GetRankingClientesFacturacionAsync(int dias = 7)
        {
            int idSucursal = 1;
            var fechaDesde = DateTimeHelper.GetArgentinaTime().AddDays(-dias);
 
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Fecha >= fechaDesde)
                .Where(p => p.IDSucursal == idSucursal)
                .ToListAsync();
 
            return pedidos
                .GroupBy(p => new { p.IDCliente, p.Cliente.Nombre })
                .Select(g => new ClienteFacturacionDTO
                {
                    NombreCliente   = g.Key.Nombre,
                    TotalFacturado  = g.Sum(p => p.Total),
                    CantidadPedidos = g.Count()
                })
                .OrderByDescending(c => c.TotalFacturado)
                .Take(10)
                .ToList();
        }
 
        public async Task<ReportePedidosCanceladosDTO> GetReportePedidosCanceladosAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            int idSucursal = 1;
            const int ID_ESTADO_CANCELADO = 9;
            var argentinaTime = DateTimeHelper.GetArgentinaTime();
            var desde = fechaDesde ?? argentinaTime.AddDays(-7);
            var hasta = (fechaHasta ?? argentinaTime).AddDays(1).AddSeconds(-1);
 
            var totalPedidos = await _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .Where(p => p.IDSucursal == idSucursal)
                .CountAsync();
 
            var pedidosCancelados = await _context.Pedidos
                .Where(p => p.IDEstadoDePedido == ID_ESTADO_CANCELADO)
                .Where(p => p.MotivoCancelacionId == null)
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .Where(p => p.IDSucursal == idSucursal)
                .ToListAsync();
 
            var totalCancelados = pedidosCancelados.Count;
            var porcentaje = totalPedidos > 0 ? (totalCancelados * 100.0m / totalPedidos) : 0m;
            var montoTotal = pedidosCancelados.Sum(p => p.Total);
 
            return new ReportePedidosCanceladosDTO
            {
                TotalPedidosCancelados = totalCancelados,
                PorcentajeDelTotal     = porcentaje,
                MontoTotalCancelado    = montoTotal,
                DetallePorMotivo       = new List<DetalleCancelacionDTO>()
            };
        }
 
        public async Task<ReporteCancelacionesPorMotivoDTO> GetReporteCancelacionesPorMotivoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            int idSucursal = 1;
            const int ID_ESTADO_CANCELADO = 9;
            var argentinaTime = DateTimeHelper.GetArgentinaTime();
            var desde = fechaDesde ?? argentinaTime.AddDays(-7);
            var hasta = (fechaHasta ?? argentinaTime).AddDays(1).AddSeconds(-1);
 
            var totalPedidos = await _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .Where(p => p.IDSucursal == idSucursal)
                .CountAsync();
 
            var pedidosCancelados = await _context.Pedidos
                .Include(p => p.MotivoCancelacion)
                .Where(p => p.IDEstadoDePedido == ID_ESTADO_CANCELADO)
                .Where(p => p.MotivoCancelacionId != null)
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .Where(p => p.IDSucursal == idSucursal)
                .ToListAsync();
 
            var totalCancelados       = pedidosCancelados.Count;
            var porcentajeCancelacion = totalPedidos > 0 ? (totalCancelados * 100.0m / totalPedidos) : 0m;
            var ingresosPerdidos      = pedidosCancelados.Sum(p => p.Total);
            var principalMotivo       = pedidosCancelados
                .GroupBy(p => p.MotivoCancelacion!.Nombre)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A";
 
            var detalleMotivos = pedidosCancelados
                .GroupBy(p => p.MotivoCancelacion!.Nombre)
                .Select(g => new CancelacionPorMotivoDTO
                {
                    Motivo       = g.Key,
                    Cantidad     = g.Count(),
                    Porcentaje   = totalCancelados > 0 ? (g.Count() * 100.0m / totalCancelados) : 0m,
                    MontoPerdido = g.Sum(p => p.Total)
                })
                .OrderByDescending(d => d.Cantidad)
                .ToList();
 
            return new ReporteCancelacionesPorMotivoDTO
            {
                TotalPedidos          = totalPedidos,
                TotalCancelados       = totalCancelados,
                PorcentajeCancelacion = porcentajeCancelacion,
                IngresosPerdidos      = ingresosPerdidos,
                PrincipalMotivo       = principalMotivo,
                DetalleMotivos        = detalleMotivos
            };
        }
 
        public async Task<List<TopProductosDTO>> GetTop10ProductosMasVendidosAsync(int dias = 7, int idSucursal = 1)
        {
            const int ID_ESTADO_CANCELADO = 9;
            DateTime fechaDesde = DateTimeHelper.GetArgentinaTime().AddDays(-dias);
 
            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Where(p => p.IDEstadoDePedido != ID_ESTADO_CANCELADO)
                .Where(p => p.Fecha >= fechaDesde)
                .Where(p => p.IDSucursal == idSucursal)
                .ToListAsync();
 
            var totalUnidades = pedidos.SelectMany(p => p.Detalles).Sum(d => d.Cantidad);
 
            return pedidos
                .SelectMany(p => p.Detalles)
                .GroupBy(d => new { d.IDProducto, d.Producto.NombreProducto })
                .Select(g => new TopProductosDTO
                {
                    IDProducto       = g.Key.IDProducto,
                    NombreProducto   = g.Key.NombreProducto,
                    UnidadesVendidas = g.Sum(d => d.Cantidad),
                    Porcentaje       = totalUnidades > 0 ? (g.Sum(d => d.Cantidad) * 100m) / totalUnidades : 0,
                    PrecioPromedio   = g.Count() > 0 ? g.Average(d => d.PrecioUnitario) : 0
                })
                .OrderByDescending(p => p.UnidadesVendidas)
                .Take(10)
                .ToList();
        }
 
        public async Task<TiemposProcesoDTO> GetReporteTiemposProcesoAsync(int dias = 7, int? idEstado = null)
        {
            int idSucursal = 1;
            DateTime fechaDesde = DateTimeHelper.GetArgentinaTime().AddDays(-dias);
 
            var query = _context.Pedidos
                .Include(p => p.HistorialDeEstados).ThenInclude(h => h.EstadoDePedido)
                .Where(p => p.Fecha >= fechaDesde)
                .Where(p => p.IDSucursal == idSucursal);
 
            if (idEstado.HasValue && idEstado.Value > 0)
            {
                Console.WriteLine($"[REPO DEBUG] Aplicando filtro de estado: {idEstado.Value}");
                query = query.Where(p => p.IDEstadoDePedido == idEstado.Value);
            }
 
            var pedidos = await query.ToListAsync();
            Console.WriteLine($"[REPO DEBUG] TiemposProceso - Total pedidos cargados: {pedidos.Count}");
 
            var detalles = new List<DetalleTiempoProcesoDTO>();
            var tiemposPorFase = new Dictionary<string, List<double>>
            {
                { "Espera",      new List<double>() },
                { "Preparación", new List<double>() },
                { "Despacho",    new List<double>() },
                { "Viaje",       new List<double>() }
            };
 
            foreach (var pedido in pedidos)
            {
                var historial = pedido.HistorialDeEstados == null
                    ? new List<HistorialDeEstados>()
                    : pedido.HistorialDeEstados.OrderBy(h => h.fecha_hora_inicio).ToList();
 
                Console.WriteLine($"[REPO DEBUG] Pedido #{pedido.IDPedido} (Estado actual: {pedido.IDEstadoDePedido}) - Historial entries: {historial.Count}");
 
                foreach (var h in historial)
                    Console.WriteLine($"  → Estado ID: {h.IDEstadoDePedido}, Nombre: {h.EstadoDePedido?.NombreEstado ?? "NULL"}, Fecha: {h.fecha_hora_inicio:yyyy-MM-dd HH:mm:ss}");
 
                var estado1 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 1);
                var estado2 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 2);
                var estado4 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 4);
                var estado6 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 6);
                var estado7 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 7);
 
                double espera = 0, preparacion = 0, despacho = 0, viaje = 0;
 
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
 
                var estadoFinal = historial.Count > 0
                    ? (historial.Last().EstadoDePedido?.NombreEstado ?? "Desconocido")
                    : "Sin historial";
 
                detalles.Add(new DetalleTiempoProcesoDTO
                {
                    IdPedido         = pedido.IDPedido,
                    Espera           = espera,
                    Preparacion      = preparacion,
                    Despacho         = despacho,
                    Viaje            = viaje,
                    EstadoFinal      = estadoFinal,
                    EsAlertaDespacho = despacho > 60
                });
            }
 
            var fases = new List<FaseProcesoDTO>
            {
                new FaseProcesoDTO { Nombre = "Espera",      TiempoPromedio = tiemposPorFase["Espera"].Count > 0      ? tiemposPorFase["Espera"].Average()      : 0, Color = "#6366f1" },
                new FaseProcesoDTO { Nombre = "Preparación", TiempoPromedio = tiemposPorFase["Preparación"].Count > 0 ? tiemposPorFase["Preparación"].Average() : 0, Color = "#8b5cf6" },
                new FaseProcesoDTO { Nombre = "Despacho",    TiempoPromedio = tiemposPorFase["Despacho"].Count > 0    ? tiemposPorFase["Despacho"].Average()    : 0, Color = "#a78bfa" },
                new FaseProcesoDTO { Nombre = "Viaje",       TiempoPromedio = tiemposPorFase["Viaje"].Count > 0       ? tiemposPorFase["Viaje"].Average()       : 0, Color = "#c4b5fd" }
            };
 
            var puntoCritico = fases.OrderByDescending(f => f.TiempoPromedio).First();
            var eficienciaDespacho = detalles.Count > 0
                ? (int)Math.Round((detalles.Count(d => d.Despacho < 30 && d.Despacho > 0) * 100.0) / detalles.Count)
                : 0;
 
            Console.WriteLine($"[REPO DEBUG] Resumen - Total: {pedidos.Count}, Detalles: {detalles.Count}, Punto Crítico: {puntoCritico.Nombre}");
 
            return new TiemposProcesoDTO
            {
                Fases              = fases,
                PuntoCritico       = puntoCritico.Nombre,
                TiempoPuntoCritico = puntoCritico.TiempoPromedio,
                EficienciaDespacho = eficienciaDespacho,
                TotalPedidos       = pedidos.Count,
                Detalles           = detalles.OrderByDescending(d => d.Despacho).ToList()
            };
        }
 
        public async Task<ReporteFormasPagoDTO> GetReporteFormasPagoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            int idSucursal = 1;
            var argentinaTime = DateTimeHelper.GetArgentinaTime();
            var desde = fechaDesde ?? argentinaTime.AddDays(-7);
            var hastaAjustado = (fechaHasta ?? argentinaTime).AddDays(1).AddSeconds(-1);
 
            var pedidos = await _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hastaAjustado)
                .Where(p => p.IDSucursal == idSucursal)
                .ToListAsync();
 
            var totalOperaciones = pedidos.Count;
            var totalMonto       = pedidos.Sum(p => p.Total);
 
            var distribucion = pedidos
                .GroupBy(p => p.FormaDePago)
                .Select(g => new DetalleFormaPagoDTO
                {
                    FormaDePago         = g.Key,
                    CantidadOperaciones = g.Count(),
                    MontoTotal          = g.Sum(p => p.Total),
                    Porcentaje          = totalOperaciones > 0 ? (g.Count() * 100m) / totalOperaciones : 0
                })
                .OrderByDescending(d => d.CantidadOperaciones)
                .ToList();
 
            return new ReporteFormasPagoDTO
            {
                TotalOperaciones       = totalOperaciones,
                TotalMonto             = totalMonto,
                DistribucionFormasPago = distribucion
            };
        }
 
        public async Task<List<PedidosPorZonaDTO>> GetReportePedidosPorZonaAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idZona = null)
        {
            var argentinaTime = DateTimeHelper.GetArgentinaTime();
            var desde = fechaDesde ?? argentinaTime.AddDays(-30);
            var hasta = fechaHasta.HasValue ? fechaHasta.Value.AddDays(1).AddSeconds(-1) : argentinaTime;
 
            var pedidosConZona = await _context.Pedidos
                .Include(p => p.Zona)
                .Include(p => p.Usuario)
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .ToListAsync();
 
            if (idZona.HasValue && idZona.Value > 0)
                pedidosConZona = pedidosConZona.Where(p => p.ZonaId == idZona.Value).ToList();
 
            if (!pedidosConZona.Any())
                return new List<PedidosPorZonaDTO>();
 
            foreach (var pedido in pedidosConZona)
            {
                var zonaId    = pedido.ZonaId.HasValue ? pedido.ZonaId.Value.ToString() : "NULL";
                var zonaNombre = pedido.Zona != null ? pedido.Zona.Nombre : "(sin zona)";
                Console.WriteLine($"Pedido {pedido.IDPedido} - ZonaId: {zonaId}, Zona.Nombre: {zonaNombre}");
            }
 
            return pedidosConZona
                .GroupBy(p => new { ZonaId = p.ZonaId ?? 0, NombreZona = p.Zona != null ? p.Zona.Nombre : "SIN ZONA" })
                .Select(g => new PedidosPorZonaDTO
                {
                    ZonaId                   = g.Key.ZonaId,
                    NombreZona               = g.Key.NombreZona,
                    CantidadPedidos          = g.Count(),
                    Porcentaje               = (g.Count() * 100.0m / pedidosConZona.Count),
                    TotalRecaudado           = g.Sum(p => p.Total),
                    EntregasExitosas         = g.Count(p => p.IDEstadoDePedido == 7),
                    EntregasFallidas         = g.Count(p => p.IDEstadoDePedido == 9),
                    PorcentajeEfectividad    = g.Count() > 0 ? (g.Count(p => p.IDEstadoDePedido == 7) * 100.0m / g.Count()) : 0m,
                    Cadetes                  = g.GroupBy(p => new { p.IDUsuario, p.Usuario!.Nombre })
                        .Select(cg => new CadeteZonaDTO
                        {
                            IDCadete              = cg.Key.IDUsuario,
                            NombreCadete          = cg.Key.Nombre,
                            TotalPedidosAsignados = cg.Count(),
                            EntregasExitosas      = cg.Count(p => p.IDEstadoDePedido == 7),
                            EntregasFallidas      = cg.Count(p => p.IDEstadoDePedido == 9),
                            TotalRecaudado        = cg.Sum(p => p.Total),
                            PorcentajeEfectividad = cg.Count() > 0 ? (cg.Count(p => p.IDEstadoDePedido == 7) * 100.0m / cg.Count()) : 0m
                        })
                        .OrderByDescending(c => c.EntregasExitosas)
                        .ToList()
                })
                .OrderByDescending(r => r.CantidadPedidos)
                .ToList();
        }
 
        public async Task<ReporteEncuestaSatisfaccionDTO> GetReporteEncuestaSatisfaccionAsync()
        {
            var csvUrl = _configuration["GoogleForms:SurveyResponsesCsvUrl"]
                ?? Environment.GetEnvironmentVariable("GOOGLE_FORMS_SURVEY_RESPONSES_CSV_URL");
 
            if (string.IsNullOrWhiteSpace(csvUrl))
                throw new InvalidOperationException(
                    "No está configurada la URL CSV de respuestas de Google Forms. Configure 'GoogleForms:SurveyResponsesCsvUrl' o 'GOOGLE_FORMS_SURVEY_RESPONSES_CSV_URL'.");
 
            var client  = _httpClientFactory.CreateClient();
            var csvData = await client.GetStringAsync(csvUrl);
 
            using var stringReader = new StringReader(csvData);
            using var csv = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound      = null,
                MissingFieldFound = null,
                HeaderValidated   = null
            });
 
            if (!csv.Read() || !csv.ReadHeader())
                return new ReporteEncuestaSatisfaccionDTO();
 
            var headers = csv.HeaderRecord ?? Array.Empty<string>();
            if (headers.Length <= 1)
                return new ReporteEncuestaSatisfaccionDTO();
 
            var resultadosPorPregunta     = new Dictionary<int, Dictionary<string, int>>();
            var totalRespuestasFormulario = 0;
 
            while (csv.Read())
            {
                var filaTieneDatos = false;
                for (int i = 1; i < headers.Length; i++)
                {
                    var header = headers[i]?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(header) || header.Contains("@")) continue;
 
                    var respuesta = csv.GetField(i)?.Trim();
                    if (string.IsNullOrWhiteSpace(respuesta)) continue;
 
                    filaTieneDatos = true;
 
                    if (!resultadosPorPregunta.TryGetValue(i, out var respuestasPregunta))
                    {
                        respuestasPregunta = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        resultadosPorPregunta[i] = respuestasPregunta;
                    }
                    respuestasPregunta[respuesta] = respuestasPregunta.GetValueOrDefault(respuesta, 0) + 1;
                }
                if (filaTieneDatos) totalRespuestasFormulario++;
            }
 
            var preguntas = new List<PreguntaEncuestaDTO>();
            for (int i = 1; i < headers.Length; i++)
            {
                var pregunta = headers[i]?.Trim();
                if (string.IsNullOrWhiteSpace(pregunta) || pregunta.Contains("@")) continue;
 
                var respuestasPregunta = resultadosPorPregunta.TryGetValue(i, out var respuestasRegistradas)
                    ? respuestasRegistradas : null;
 
                var totalPorPregunta = respuestasPregunta?.Values.Sum() ?? 0;
 
                // Filtrar preguntas sin respuestas
                if (totalPorPregunta == 0) continue;
 
                var opciones = (respuestasPregunta ?? new Dictionary<string, int>())
                    .Select(x => new OpcionRespuestaEncuestaDTO
                    {
                        Respuesta  = x.Key,
                        Cantidad   = x.Value,
                        Porcentaje = totalPorPregunta > 0 ? Math.Round((x.Value * 100m) / totalPorPregunta, 2) : 0
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ThenBy(x => x.Respuesta)
                    .ToList();
 
                preguntas.Add(new PreguntaEncuestaDTO
                {
                    Pregunta        = pregunta,
                    TotalRespuestas = totalPorPregunta,
                    Opciones        = opciones
                });
            }
 
            // Contar pedidos entregados (estado 7) - incluye tanto normales como demorados
            var cantidadEncuestasEnviadas = await _context.Pedidos
                .Where(p => p.IDEstadoDePedido == 7)
                .CountAsync();

            return new ReporteEncuestaSatisfaccionDTO
            {
                CantidadClientesRespondieron = totalRespuestasFormulario,
                CantidadTotalRespuestas      = preguntas.Sum(p => p.Opciones.Sum(o => o.Cantidad)),
                CantidadEncuestasEnviadas    = cantidadEncuestasEnviadas,
                Preguntas                    = preguntas
            };
        }
 
        /// <summary>
        /// Reporte de entregas fuera de plazo.
        /// Solo incluye pedidos Entregados (estado 7) que superaron la FechaEntregaEstimada.
        /// </summary>
        public async Task<PedidosFueraDeplazoDTO> GetReportePedidosFueraDeplazoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            var argentinaTime = DateTimeHelper.GetArgentinaTime();
            var desde = fechaDesde ?? argentinaTime.AddDays(-30);
            var hasta = (fechaHasta ?? argentinaTime).AddDays(1).AddSeconds(-1);
 
            var pedidosEntregados = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Where(p => p.IDEstadoDePedido == 7)
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .ToListAsync();
 
            // Filtrar solo los pedidos que fueron marcados como demorados
            var pedidosFueraDeplazo = pedidosEntregados
                .Where(p => p.EsDemorado)
                .ToList();
 
            var detalles = pedidosFueraDeplazo
                .Select(p => new DetallePedidoFueraDeplazo
                {
                    IDPedido      = p.IDPedido,
                    ClienteNombre = p.Cliente != null
                        ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}"
                        : "Consumidor Final",
                    NombreCadete  = p.Usuario != null
                        ? $"{p.Usuario.Nombre} {p.Usuario.Apellido}"
                        : "Sin asignar",
                    FechaCreacion = p.Fecha,
                    FechaEstimada = p.FechaEntregaEstimada,
                    FechaEntrega  = p.FechaEntregaReal ?? argentinaTime,
                    RetrasoDías   = (int)Math.Round(
                        DateTimeHelper.CalcularDiasHabilesDiferencia(
                            p.FechaEntregaEstimada,
                            p.FechaEntregaReal ?? argentinaTime)),
                    IntentosEntregaFallida = p.IntentosEntregaFallida,
                    EsDemorado             = p.EsDemorado,
                    FechaMarcadoDemorado   = p.FechaMarcadoDemorado
                })
                .OrderByDescending(d => d.RetrasoDías)
                .ToList();
 
            var retrasoPromedio = detalles.Any()
                ? (int)Math.Round(detalles.Average(d => d.RetrasoDías))
                : 0;
 
            return new PedidosFueraDeplazoDTO
            {
                TotalEntregas       = pedidosEntregados.Count,
                EntregasTardías     = pedidosFueraDeplazo.Count,
                RetrasoPromedioDías = retrasoPromedio,
                Detalles            = detalles
            };
        }
 
        /// <summary>
        /// Devuelve TODOS los pedidos con EsDemorado=true sin filtrar por estado.
        /// Usado por el dashboard del encargado para el conteo histórico.
        /// </summary>
        public async Task<List<OrderSummaryDTO>> GetPedidosDemoradosAsync()
        {
            var pedidosDemorados = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.EstadoDePedido)
                .Where(p => p.EsDemorado)
                .OrderBy(p => p.FechaEntregaEstimada)
                .ToListAsync();
 
            return pedidosDemorados.Select(p => new OrderSummaryDTO
            {
                IDPedido             = p.IDPedido,
                ClienteNombre        = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Consumidor Final",
                EstadoNombre         = p.EstadoDePedido != null ? p.EstadoDePedido.NombreEstado : "Desconocido",
                IDEstadoDePedido     = p.IDEstadoDePedido,
                Total                = p.Total,
                Fecha                = p.Fecha,
                EstaDemorado         = true,
                FechaEntregaEstimada = p.FechaEntregaEstimada,
                FechaMarcadoDemorado = p.FechaMarcadoDemorado,
                ResponsableNombre    = p.Usuario != null ? $"{p.Usuario.Nombre} {p.Usuario.Apellido}" : "Sin asignar",
                ResponsableRol       = p.Usuario?.Rol ?? string.Empty
            }).ToList();
        }
 
        // No-op: la lógica de marcado vive exclusivamente en PedidosDemoradosBackgroundService
        private Task MarcarPedidosDemoradosAutomaticamenteAsync() => Task.CompletedTask;
 
        /// <summary>
        /// Devuelve pedidos demorados ACTIVOS filtrados según el rol del usuario logueado.
        /// Usado por la campanita — excluye pedidos finalizados:
        ///   - Estado 7: Entregado
        ///   - Estado 9: Cancelado (manual o automático por 3 intentos fallidos)
        ///   - Estado 8 con IntentosEntregaFallida >= 3: defensa por inconsistencia en DB
        /// EsDemorado sigue siendo true en DB para reportes históricos.
        /// </summary>
        public async Task<List<OrderSummaryDTO>> GetPedidosDemoradosPorUsuarioAsync(int usuarioId, string rolUsuario)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.EstadoDePedido)
                .Where(p => p.EsDemorado)
                // Excluir estados finales para que desaparezcan de la campanita.
                // EsDemorado permanece true en la DB para reportes históricos.
                .Where(p => p.IDEstadoDePedido != 7   // No Entregado
                         && p.IDEstadoDePedido != 9   // No Cancelado (incluye cancelación por 3 intentos)
                         && !(p.IDEstadoDePedido == 8 && p.IntentosEntregaFallida >= 3)) // defensa
                .AsQueryable();
 
            if (rolUsuario == "Operario")
            {
                query = query.Where(p => p.IDUsuario == usuarioId);
            }
            else if (rolUsuario == "Cadete")
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null) return new List<OrderSummaryDTO>();
                var zonaId = usuario.ZonaId;
                query = query.Where(p => p.ZonaId == zonaId);
            }
            // Encargado: sin filtro adicional, ve todos los demorados activos
 
            var pedidosDemorados = await query
                .OrderBy(p => p.FechaEntregaEstimada)
                .ToListAsync();
 
            return pedidosDemorados.Select(p => new OrderSummaryDTO
            {
                IDPedido             = p.IDPedido,
                ClienteNombre        = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Consumidor Final",
                EstadoNombre         = p.EstadoDePedido != null ? p.EstadoDePedido.NombreEstado : "Desconocido",
                IDEstadoDePedido     = p.IDEstadoDePedido,
                Total                = p.Total,
                Fecha                = p.Fecha,
                EstaDemorado         = true,
                FechaEntregaEstimada = p.FechaEntregaEstimada,
                FechaMarcadoDemorado = p.FechaMarcadoDemorado,
                ResponsableNombre    = p.Usuario != null ? $"{p.Usuario.Nombre} {p.Usuario.Apellido}" : "Sin asignar",
                ResponsableRol       = p.Usuario?.Rol ?? string.Empty
            }).ToList();
        }
    }
}