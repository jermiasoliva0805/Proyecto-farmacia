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
        public async Task<IActionResult> GetEntregasPorCadete([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
        {
            try
            {
                // Si no se envían fechas, por defecto tomamos el último mes
                if (fechaDesde == default) fechaDesde = DateTime.Now.AddMonths(-1);
                if (fechaHasta == default) fechaHasta = DateTime.Now;

                var reporte = await _reporteRepository.GetReporteEntregasPorCadeteAsync(fechaDesde, fechaHasta);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al generar el reporte", error = ex.Message });
            }
        }


        // clientes frecuentes por volumen jere.
        [HttpGet("ranking-clientes")]
        public async Task<IActionResult> GetRankingClientes()
        {
            try
            {
                var reporte = await _reporteRepository.GetRankingClientesFrecuentesAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al generar el ranking de clientes", error = ex.Message });
            }
        }
    }


    

}