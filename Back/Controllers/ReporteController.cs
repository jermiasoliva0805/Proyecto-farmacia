using Back.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
                // Si no se envían fechas, por defecto tomamos los últimos 7 días
                var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
                var hasta = fechaHasta ?? DateTime.Now;

                var reporte = await _reporteRepository.GetReporteEntregasPorCadeteAsync(desde, hasta, idSucursal);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al generar el reporte", error = ex.Message });
            }
        }
    }
}