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
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            try
            {
                // Si no se envían fechas, por defecto tomamos el último mes
                var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
                var hasta = fechaHasta ?? DateTime.Now;

                if (desde > hasta)
                    return BadRequest(new { message = "La fechaDesde no puede ser mayor que fechaHasta" });

                var reporte = await _reporteRepository.GetReporteEntregasPorCadeteAsync(desde, hasta);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el reporte", error = ex.Message });
            }
        }
    }
}
