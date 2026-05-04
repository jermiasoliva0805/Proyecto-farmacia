using Back.Repositories.Interfaces;
using Back.DTOs;
using Back.Utils;
using Microsoft.AspNetCore.Mvc;
using Proyecto_farmacia.DTOs;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteRepository _reporteRepository;
        private readonly AppDbContext _context;

        public ReporteController(IReporteRepository reporteRepository, AppDbContext context)
        {
            _reporteRepository = reporteRepository;
            _context = context;
        }

        [HttpGet("entregas-cadete")]
        public async Task<IActionResult> GetEntregasPorCadete(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var usuarioId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                // Obtener la sucursal del usuario
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
                var hasta = fechaHasta ?? DateTime.Now;

                System.Diagnostics.Debug.WriteLine($"[REPORTE] UsuarioID: {usuarioId}, SucursalID: {usuario.IDSucursal}, Desde: {desde}, Hasta: {hasta}");

                var reporte = await _reporteRepository.GetReporteEntregasPorCadeteAsync(desde, hasta, usuario.IDSucursal);

                System.Diagnostics.Debug.WriteLine($"[REPORTE] Cadetes encontrados: {reporte.Count}");

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[REPORTE] Error: {ex.Message} - {ex.StackTrace}");
                return BadRequest(new { message = "Error al generar el reporte", error = ex.Message });
            }
        }

        [HttpGet("ranking-clientes")]
        public async Task<IActionResult> GetRankingClientes(
            [FromQuery] int dias = 7)
        {
            try
            {
                var reporte = await _reporteRepository.GetRankingClientesFrecuentesAsync(dias);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error en ranking", error = ex.Message });
            }
        }

        [HttpGet("clientes-facturacion")]
        public async Task<ActionResult<List<ClienteFacturacionDTO>>> GetRankingFacturacion(
            [FromQuery] int dias = 7)
        {
            try
            {
                var reporte = await _reporteRepository.GetRankingClientesFacturacionAsync(dias);

                if (reporte == null)
                {
                    return Ok(new List<ClienteFacturacionDTO>());
                }

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener el reporte de facturación: {ex.Message}");
            }
        }

        [HttpGet("pedidos-cancelados")]
        public async Task<ActionResult<ReportePedidosCanceladosDTO>> GetPedidosCancelados(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var fechaDesdeNormalizada = fechaDesde.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaDesde.Value) 
                    : (DateTime?)null;
                var fechaHastaNormalizada = fechaHasta.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaHasta.Value) 
                    : (DateTime?)null;

                var reporte = await _reporteRepository.GetReportePedidosCanceladosAsync(fechaDesdeNormalizada, fechaHastaNormalizada);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de pedidos cancelados", error = ex.Message });
            }
        }

        [HttpGet("cancelaciones-por-motivo")]
        public async Task<ActionResult<ReporteCancelacionesPorMotivoDTO>> GetCancelacionesPorMotivo(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var fechaDesdeNormalizada = fechaDesde.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaDesde.Value) 
                    : (DateTime?)null;
                var fechaHastaNormalizada = fechaHasta.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaHasta.Value) 
                    : (DateTime?)null;

                var reporte = await _reporteRepository.GetReporteCancelacionesPorMotivoAsync(fechaDesdeNormalizada, fechaHastaNormalizada);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de cancelaciones por motivo", error = ex.Message });
            }
        }

        [HttpGet("top-productos")]
        public async Task<ActionResult<List<TopProductosDTO>>> GetTop10Productos(
            [FromQuery] int dias = 7)
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var usuarioId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                // Obtener la sucursal del usuario
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                var reporte = await _reporteRepository.GetTop10ProductosMasVendidosAsync(dias, usuario.IDSucursal);

                if (reporte == null || reporte.Count == 0)
                {
                    return Ok(new List<TopProductosDTO>());
                }

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de productos", error = ex.Message });
            }
        }

        [HttpGet("tiempos-proceso")]
        public async Task<ActionResult<TiemposProcesoDTO>> GetReporteTiemposProceso(
            [FromQuery] int dias = 7,
            [FromQuery] int? idEstado = null)
        {
            try
            {
                var reporte = await _reporteRepository.GetReporteTiemposProcesoAsync(dias, idEstado);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de tiempos", error = ex.Message });
            }
        }

        [HttpGet("formas-pago")]
        public async Task<ActionResult<ReporteFormasPagoDTO>> GetReporteFormasPago(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var fechaDesdeNormalizada = fechaDesde.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaDesde.Value) 
                    : (DateTime?)null;
                var fechaHastaNormalizada = fechaHasta.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaHasta.Value) 
                    : (DateTime?)null;

                var reporte = await _reporteRepository.GetReporteFormasPagoAsync(fechaDesdeNormalizada, fechaHastaNormalizada);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de formas de pago", error = ex.Message });
            }
        }

        [HttpGet("pedidos-por-zona")]
        public async Task<ActionResult<List<PedidosPorZonaDTO>>> GetPedidosPorZona(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? idZona = null)
        {
            try
            {
                var fechaDesdeNormalizada = fechaDesde.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaDesde.Value) 
                    : (DateTime?)null;
                var fechaHastaNormalizada = fechaHasta.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaHasta.Value) 
                    : (DateTime?)null;

                var reporte = await _reporteRepository.GetReportePedidosPorZonaAsync(fechaDesdeNormalizada, fechaHastaNormalizada, idZona);

                if (reporte == null)
                {
                    return Ok(new List<PedidosPorZonaDTO>());
                }

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de pedidos por zona", error = ex.Message });
            }
        }

        [HttpGet("debug-pedidos-por-zona")]
        public async Task<IActionResult> GetDebugPedidosPorZona(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var desde = fechaDesde.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaDesde.Value)
                    : DateTimeHelper.GetArgentinaTime().AddDays(-30);
                var hasta = fechaHasta.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaHasta.Value).AddDays(1).AddSeconds(-1)
                    : DateTimeHelper.GetArgentinaTime().AddDays(1).AddSeconds(-1);

                var pedidos = await _context.Pedidos
                    .Include(p => p.Zona)
                    .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
                    .OrderBy(p => p.ZonaId)
                    .ThenByDescending(p => p.Fecha)
                    .Select(p => new
                    {
                        p.IDPedido,
                        p.ZonaId,
                        NombreZona = p.Zona != null ? p.Zona.Nombre : "SIN ZONA",
                        p.Fecha,
                        p.Total
                    })
                    .ToListAsync();

                Console.WriteLine($"\n[DEBUG PEDIDOS] Rango: {desde:yyyy-MM-dd} a {hasta:yyyy-MM-dd}");
                Console.WriteLine($"[DEBUG PEDIDOS] Total de pedidos: {pedidos.Count}");

                var agrupadosPorZona = new Dictionary<string, List<dynamic>>();
                
                foreach (var pedido in pedidos)
                {
                    string clave = pedido.ZonaId?.ToString() ?? "NULL";
                    if (!agrupadosPorZona.ContainsKey(clave))
                    {
                        agrupadosPorZona[clave] = new List<dynamic>();
                    }
                    agrupadosPorZona[clave].Add(pedido);
                }
                
                Console.WriteLine($"[DEBUG PEDIDOS] Agrupado por ZonaId:");
                foreach (var grupo in agrupadosPorZona)
                {
                    Console.WriteLine($"  ZonaId {grupo.Key}: {grupo.Value.Count} pedidos");
                }

                var resultado = new
                {
                    fechaDesde = desde.ToString("yyyy-MM-dd"),
                    fechaHasta = hasta.ToString("yyyy-MM-dd"),
                    totalPedidos = pedidos.Count,
                    agrupadoPorZona = agrupadosPorZona.Select(g => new
                    {
                        zonaId = g.Key,
                        cantidad = g.Value.Count,
                        pedidos = g.Value
                    }).ToList(),
                    detallePedidos = pedidos
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG ERROR] {ex.Message}");
                Console.WriteLine($"[DEBUG ERROR] {ex.StackTrace}");
                return BadRequest(new { message = "Error al obtener debug", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("encuesta-satisfaccion")]
        public async Task<ActionResult<ReporteEncuestaSatisfaccionDTO>> GetReporteEncuestaSatisfaccion()
        {
            try
            {
                var reporte = await _reporteRepository.GetReporteEncuestaSatisfaccionAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de encuesta de satisfacción", error = ex.Message });
            }
        }

        [HttpGet("entregas-fuera-de-plazo")]
        public async Task<ActionResult<PedidosFueraDeplazoDTO>> GetPedidosFueraDeplazo(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                // Normalizar fechas a hora Argentina si vienen del frontend
                var fechaDesdeNormalizada = fechaDesde.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaDesde.Value) 
                    : (DateTime?)null;
                var fechaHastaNormalizada = fechaHasta.HasValue 
                    ? DateTimeHelper.InterpretarComoArgentina(fechaHasta.Value) 
                    : (DateTime?)null;

                var reporte = await _reporteRepository.GetReportePedidosFueraDeplazoAsync(fechaDesdeNormalizada, fechaHastaNormalizada);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de entregas fuera de plazo", error = ex.Message });
            }
        }

        [HttpGet("pedidos-demorados")]
        public async Task<ActionResult<List<OrderSummaryDTO>>> GetPedidosDemorados()
        {
            try
            {
                var pedidosDemorados = await _reporteRepository.GetPedidosDemoradosAsync();
                return Ok(pedidosDemorados);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener pedidos demorados", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene pedidos demorados del usuario logueado según su rol.
        /// El filtrado se realiza automáticamente según:
        /// - Encargado: Ve todos los pedidos demorados
        /// - Operario: Ve solo sus pedidos demorados
        /// - Cadete: Ve solo los pedidos demorados de su zona
        /// Endpoint usado por la campanita de notificaciones en el frontend
        /// </summary>
        [HttpGet("pedidos-demorados-usuario")]
        public async Task<ActionResult<List<OrderSummaryDTO>>> GetPedidosDemoradosUsuario()
        {
            try
            {
                // Obtener el ID y rol del usuario del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out var usuarioId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                if (string.IsNullOrEmpty(rolClaim))
                {
                    return Unauthorized(new { message = "No se pudo determinar el rol del usuario" });
                }

                var pedidosDemorados = await _reporteRepository.GetPedidosDemoradosPorUsuarioAsync(usuarioId, rolClaim);
                return Ok(pedidosDemorados);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener pedidos demorados del usuario", error = ex.Message });
            }
        }
    }
}
