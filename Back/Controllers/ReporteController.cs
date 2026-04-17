using Back.Repositories.Interfaces;
using Back.DTOs;
using Microsoft.AspNetCore.Mvc;
using Proyecto_farmacia.DTOs;
using System;
using System.Threading.Tasks;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteRepository _reporteRepository;

        public ReporteController(IReporteRepository reporteRepository)
        {
            _reporteRepository = reporteRepository;
        }

        [HttpGet("entregas-cadete")]
        public async Task<IActionResult> GetEntregasPorCadete(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? idSucursal = null)
        {
            try
            {
                // LOGS DE DEBUG
                Console.WriteLine($"[CONTROLLER] fechaDesde: {fechaDesde}");
                Console.WriteLine($"[CONTROLLER] fechaHasta: {fechaHasta}");
                Console.WriteLine($"[CONTROLLER] idSucursal: {idSucursal}");

                var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
                var hasta = fechaHasta ?? DateTime.Now;

                Console.WriteLine($"[CONTROLLER] Calculado desde: {desde}");
                Console.WriteLine($"[CONTROLLER] Calculado hasta: {hasta}");

                var reporte = await _reporteRepository.GetReporteEntregasPorCadeteAsync(desde, hasta, idSucursal);

                Console.WriteLine($"[CONTROLLER] Reporte devuelto: {reporte.Count} cadetes");

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERROR: {ex.Message}");
                Console.WriteLine($"[CONTROLLER] Stack: {ex.StackTrace}");
                return BadRequest(new { message = "Error al generar el reporte", error = ex.Message });
            }
        }

        [HttpGet("ranking-clientes")]
        public async Task<IActionResult> GetRankingClientes(
            [FromQuery] int dias = 7,
            [FromQuery] int? idSucursal = null)
        {
            try
            {
                var reporte = await _reporteRepository.GetRankingClientesFrecuentesAsync(dias, idSucursal);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error en ranking", error = ex.Message });
            }
        }

        [HttpGet("clientes-facturacion")]
        public async Task<ActionResult<List<ClienteFacturacionDTO>>> GetRankingFacturacion(
            [FromQuery] int dias = 7,
            [FromQuery] int? idSucursal = null)
        {
            try
            {
                // Llamamos al método que ahora acepta filtros
                var reporte = await _reporteRepository.GetRankingClientesFacturacionAsync(dias, idSucursal);

                // Si no hay datos, devolvemos una lista vacía pero con status 200
                if (reporte == null)
                {
                    return Ok(new List<ClienteFacturacionDTO>());
                }

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                // Es importante loguear el error por si algo falla en el servidor
                return BadRequest($"Error al obtener el reporte de facturación: {ex.Message}");
            }
        }

        // ====== TU RAMA (ReportesNuevos2) ======

        [HttpGet("pedidos-cancelados")]
        public async Task<ActionResult<ReportePedidosCanceladosDTO>> GetPedidosCancelados(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? idSucursal = null)
        {
            try
            {
                var reporte = await _reporteRepository.GetReportePedidosCanceladosAsync(fechaDesde, fechaHasta, idSucursal);
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
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? idSucursal = null)
        {
            try
            {
                var reporte = await _reporteRepository.GetReporteCancelacionesPorMotivoAsync(fechaDesde, fechaHasta, idSucursal);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de cancelaciones por motivo", error = ex.Message });
            }
        }

        // ====== MAIN ======

        [HttpGet("top-productos")]
        public async Task<ActionResult<List<TopProductosDTO>>> GetTop10Productos(
            [FromQuery] int dias = 7,
            [FromQuery] int? idSucursal = null)
        {
            try
            {
                var reporte = await _reporteRepository.GetTop10ProductosMasVendidosAsync(dias, idSucursal);

                if (reporte == null)
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
            [FromQuery] int? idSucursal = null,
            [FromQuery] int? idEstado = null)
        {
            try
            {
                var reporte = await _reporteRepository.GetReporteTiemposProcesoAsync(dias, idSucursal, idEstado);
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
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? idSucursal = null)
        {
            try
            {
                var reporte = await _reporteRepository.GetReporteFormasPagoAsync(fechaDesde, fechaHasta, idSucursal);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el reporte de formas de pago", error = ex.Message });
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
    }
}
