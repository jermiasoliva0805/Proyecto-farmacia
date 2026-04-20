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
            int? idSucursal = null)
        {
            var hasta = fechaHasta.AddDays(1).AddSeconds(-1);

            var query = _context.Pedidos
                .Include(p => p.Usuario)
                .Where(p => p.Usuario != null && p.Usuario.Rol == "Cadete")
                .Where(p => p.Usuario != null && !p.Usuario.IsDeleted)
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

            var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
            var hasta = fechaHasta ?? DateTime.Now;

            hasta = hasta.AddDays(1).AddSeconds(-1);

            var queryTodosPedidos = _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

            var queryCancelados = _context.Pedidos
                .Where(p => p.IDEstadoDePedido == ID_ESTADO_CANCELADO)
                .Where(p => p.MotivoCancelacionId == null)
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                queryTodosPedidos = queryTodosPedidos.Where(p => p.IDSucursal == idSucursal.Value);
                queryCancelados = queryCancelados.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var totalPedidos = await queryTodosPedidos.CountAsync();
            var pedidosCancelados = await queryCancelados.ToListAsync();
            var totalCancelados = pedidosCancelados.Count;

            var porcentaje = totalPedidos > 0 ? (totalCancelados * 100.0m / totalPedidos) : 0m;

            var montoTotal = pedidosCancelados.Sum(p => p.Total);

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

            var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
            var hasta = fechaHasta ?? DateTime.Now;

            hasta = hasta.AddDays(1).AddSeconds(-1);

            var queryTodosPedidos = _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

            var queryCancelados = _context.Pedidos
                .Include(p => p.MotivoCancelacion)
                .Where(p => p.IDEstadoDePedido == ID_ESTADO_CANCELADO)
                .Where(p => p.MotivoCancelacionId != null)
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .AsQueryable();

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

            var principalMotivo = pedidosCancelados
                .GroupBy(p => p.MotivoCancelacion!.Nombre)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A";

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
            const int ID_ESTADO_CANCELADO = 9;
            
            DateTime fechaDesde = DateTime.Now.AddDays(-dias);

            var query = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .Where(p => p.IDEstadoDePedido != ID_ESTADO_CANCELADO)
                .Where(p => p.Fecha >= fechaDesde);

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var pedidos = await query.ToListAsync();

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
            DateTime fechaDesde = DateTime.Now.AddDays(-dias);

            var query = _context.Pedidos
                .Include(p => p.HistorialDeEstados)
                    .ThenInclude(h => h.EstadoDePedido)
                .Where(p => p.Fecha >= fechaDesde);

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

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

                var estado1 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 1);
                var estado2 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 2);
                var estado4 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 4);
                var estado6 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 6);
                var estado7 = historial.FirstOrDefault(h => h.IDEstadoDePedido == 7);

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

                var estadoFinal = historial.Count > 0 
                    ? (historial.Last().EstadoDePedido?.NombreEstado ?? "Desconocido") 
                    : "Sin historial";

                detalles.Add(new DetalleTiempoProcesoDTO
                {
                    IdPedido = pedido.IDPedido,
                    Espera = espera,
                    Preparacion = preparacion,
                    Despacho = despacho,
                    Viaje = viaje,
                    EstadoFinal = estadoFinal,
                    EsAlertaDespacho = despacho > 60
                });
            }

            var fases = new List<FaseProcesoDTO>
            {
                new FaseProcesoDTO
                {
                    Nombre = "Espera",
                    TiempoPromedio = tiemposPorFase["Espera"].Count > 0 ? tiemposPorFase["Espera"].Average() : 0,
                    Color = "#6366f1"
                },
                new FaseProcesoDTO
                {
                    Nombre = "Preparación",
                    TiempoPromedio = tiemposPorFase["Preparación"].Count > 0 ? tiemposPorFase["Preparación"].Average() : 0,
                    Color = "#8b5cf6"
                },
                new FaseProcesoDTO
                {
                    Nombre = "Despacho",
                    TiempoPromedio = tiemposPorFase["Despacho"].Count > 0 ? tiemposPorFase["Despacho"].Average() : 0,
                    Color = "#a78bfa"
                },
                new FaseProcesoDTO
                {
                    Nombre = "Viaje",
                    TiempoPromedio = tiemposPorFase["Viaje"].Count > 0 ? tiemposPorFase["Viaje"].Average() : 0,
                    Color = "#c4b5fd"
                }
            };

            var puntoCritico = fases.OrderByDescending(f => f.TiempoPromedio).First();

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

        public async Task<ReporteFormasPagoDTO> GetReporteFormasPagoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null)
        {
            var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
            var hasta = fechaHasta ?? DateTime.Now;
            var hastaAjustado = hasta.AddDays(1).AddSeconds(-1);

            var query = _context.Pedidos
                .Where(p => p.Fecha >= desde && p.Fecha <= hastaAjustado)
                .AsQueryable();

            if (idSucursal.HasValue && idSucursal.Value > 0)
            {
                query = query.Where(p => p.IDSucursal == idSucursal.Value);
            }

            var pedidos = await query.ToListAsync();

            var totalOperaciones = pedidos.Count;
            var totalMonto = pedidos.Sum(p => p.Total);

            var distribucion = pedidos
                .GroupBy(p => p.FormaDePago)
                .Select(g => new DetalleFormaPagoDTO
                {
                    FormaDePago = g.Key,
                    CantidadOperaciones = g.Count(),
                    MontoTotal = g.Sum(p => p.Total),
                    Porcentaje = totalOperaciones > 0 ? (g.Count() * 100m) / totalOperaciones : 0
                })
                .OrderByDescending(d => d.CantidadOperaciones)
                .ToList();

            return new ReporteFormasPagoDTO
            {
                TotalOperaciones = totalOperaciones,
                TotalMonto = totalMonto,
                DistribucionFormasPago = distribucion
            };
        }

        public async Task<List<PedidosPorZonaDTO>> GetReportePedidosPorZonaAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idZona = null)
        {
            var desde = fechaDesde ?? DateTime.Now.AddDays(-30);
            var hasta = fechaHasta.HasValue ? fechaHasta.Value.AddDays(1).AddSeconds(-1) : DateTime.Now;

            var pedidosConZona = await _context.Pedidos
                .Include(p => p.Zona)
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .ToListAsync();

            if (idZona.HasValue && idZona.Value > 0)
            {
                pedidosConZona = pedidosConZona.Where(p => p.ZonaId == idZona.Value).ToList();
            }

            if (!pedidosConZona.Any())
                return new List<PedidosPorZonaDTO>();

            foreach (var pedido in pedidosConZona)
            {
                var zonaId = pedido.ZonaId.HasValue ? pedido.ZonaId.Value.ToString() : "NULL";
                var zonaNombre = pedido.Zona != null ? pedido.Zona.Nombre : "(sin zona)";
                Console.WriteLine($"Pedido {pedido.IDPedido} - ZonaId: {zonaId}, Zona.Nombre: {zonaNombre}");
            }

            // FIX: agrupar por valores escalares (ZonaId y NombreZona) en lugar del objeto Zona
            var reporte = pedidosConZona
                .GroupBy(p => new {
                    ZonaId = p.ZonaId ?? 0,
                    NombreZona = p.Zona != null ? p.Zona.Nombre : "SIN ZONA"
                })
                .Select(g => new PedidosPorZonaDTO
                {
                    ZonaId = g.Key.ZonaId,
                    NombreZona = g.Key.NombreZona,
                    CantidadPedidos = g.Count(),
                    Porcentaje = (g.Count() * 100.0m / pedidosConZona.Count),
                    TotalRecaudado = g.Sum(p => p.Total)
                })
                .OrderByDescending(r => r.CantidadPedidos)
                .ToList();

            return reporte;
        }

        public async Task<ReporteEncuestaSatisfaccionDTO> GetReporteEncuestaSatisfaccionAsync()
        {
            var csvUrl =
                _configuration["GoogleForms:SurveyResponsesCsvUrl"]
                ?? Environment.GetEnvironmentVariable("GOOGLE_FORMS_SURVEY_RESPONSES_CSV_URL");

            if (string.IsNullOrWhiteSpace(csvUrl))
            {
                throw new InvalidOperationException(
                    "No está configurada la URL CSV de respuestas de Google Forms. Configure 'GoogleForms:SurveyResponsesCsvUrl' o 'GOOGLE_FORMS_SURVEY_RESPONSES_CSV_URL'.");
            }

            var client = _httpClientFactory.CreateClient();
            var csvData = await client.GetStringAsync(csvUrl);

            using var stringReader = new StringReader(csvData);
            using var csv = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                MissingFieldFound = null,
                HeaderValidated = null
            });

            if (!csv.Read() || !csv.ReadHeader())
            {
                return new ReporteEncuestaSatisfaccionDTO();
            }

            var headers = csv.HeaderRecord ?? Array.Empty<string>();

            if (headers.Length <= 1)
            {
                return new ReporteEncuestaSatisfaccionDTO();
            }

            var resultadosPorPregunta = new Dictionary<int, Dictionary<string, int>>();
            var totalRespuestasFormulario = 0;

            while (csv.Read())
            {
                var filaTieneDatos = false;

                for (int i = 1; i < headers.Length; i++)
                {
                    var respuesta = csv.GetField(i)?.Trim();
                    if (string.IsNullOrWhiteSpace(respuesta))
                    {
                        continue;
                    }

                    filaTieneDatos = true;

                    if (!resultadosPorPregunta.TryGetValue(i, out var respuestasPregunta))
                    {
                        respuestasPregunta = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        resultadosPorPregunta[i] = respuestasPregunta;
                    }

                    respuestasPregunta[respuesta] = respuestasPregunta.GetValueOrDefault(respuesta, 0) + 1;
                }

                if (filaTieneDatos)
                {
                    totalRespuestasFormulario++;
                }
            }

            var preguntas = new List<PreguntaEncuestaDTO>();

            for (int i = 1; i < headers.Length; i++)
            {
                var pregunta = headers[i]?.Trim();
                if (string.IsNullOrWhiteSpace(pregunta))
                {
                    continue;
                }

                var respuestasPregunta = resultadosPorPregunta.TryGetValue(i, out var respuestasRegistradas)
                    ? respuestasRegistradas
                    : null;

                var totalPorPregunta = respuestasPregunta?.Values.Sum() ?? 0;
                var opciones = (respuestasPregunta ?? new Dictionary<string, int>())
                    .Select(x => new OpcionRespuestaEncuestaDTO
                    {
                        Respuesta = x.Key,
                        Cantidad = x.Value,
                        Porcentaje = totalPorPregunta > 0
                            ? Math.Round((x.Value * 100m) / totalPorPregunta, 2)
                            : 0
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ThenBy(x => x.Respuesta)
                    .ToList();

                preguntas.Add(new PreguntaEncuestaDTO
                {
                    Pregunta = pregunta,
                    TotalRespuestas = totalPorPregunta,
                    Opciones = opciones
                });
            }

            return new ReporteEncuestaSatisfaccionDTO
            {
                TotalRespuestas = totalRespuestasFormulario,
                Preguntas = preguntas
            };
        }

        /// <summary>
        /// Obtiene el reporte de entregas fuera de plazo (entregadas después de la fecha estimada)
        /// Solo incluye pedidos con estado 7 (Entregado)
        /// </summary>
        public async Task<PedidosFueraDeplazoDTO> GetReportePedidosFueraDeplazoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            // Configurar rango de fechas
            var desde = fechaDesde ?? DateTime.Now.AddDays(-30);
            var hasta = (fechaHasta ?? DateTime.Now).AddDays(1).AddSeconds(-1);

            // Obtener pedidos entregados en el rango
            var pedidosEntregados = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Where(p => p.IDEstadoDePedido == 7) // Entregado
                .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                .ToListAsync();

            // Filtrar solo los que fueron entregados después de la fecha estimada
            var pedidosFueraDeplazo = pedidosEntregados
                .Where(p => p.FechaEntregaReal.HasValue && 
                           p.FechaEntregaReal.Value > p.FechaEntregaEstimada)
                .ToList();

            // Construir los detalles
            var detalles = pedidosFueraDeplazo
                .Select(p => new DetallePedidoFueraDeplazo
                {
                    IDPedido = p.IDPedido,
                    ClienteNombre = p.Cliente != null ? $"{p.Cliente.Nombre} {p.Cliente.Apellido}" : "Consumidor Final",
                    NombreCadete = p.Usuario != null ? $"{p.Usuario.Nombre} {p.Usuario.Apellido}" : "Sin asignar",
                    FechaCreacion = p.Fecha,
                    FechaEstimada = p.FechaEntregaEstimada,
                    FechaEntrega = p.FechaEntregaReal ?? DateTime.Now,
                    RetrasoDías = (int)Math.Ceiling(DateTimeHelper.CalcularDiasHabilesDiferencia(p.FechaEntregaEstimada, p.FechaEntregaReal ?? DateTime.Now)),
                    IntentosEntregaFallida = p.IntentosEntregaFallida
                })
                .OrderByDescending(d => d.RetrasoDías) // Ordenar por mayor retraso
                .ToList();

            // Calcular métricas
            var retrasoPromedio = detalles.Any() ? (int)Math.Ceiling(detalles.Average(d => d.RetrasoDías)) : 0;

            return new PedidosFueraDeplazoDTO
            {
                TotalEntregas = pedidosEntregados.Count,
                EntregasTardías = pedidosFueraDeplazo.Count,
                RetrasoPromedioDías = retrasoPromedio,
                Detalles = detalles
            };
        }
    }
}
