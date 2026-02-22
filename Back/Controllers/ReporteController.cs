using Back.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
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
    }
}