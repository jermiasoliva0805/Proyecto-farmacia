using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly ITrackingService _trackingService;

        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        // GET: api/tracking/{idPedido}
        // Motivo: Cumple con el Mandato de trazabilidad, mostrando el historial de estados del pedido.
        [HttpGet("{idPedido}")]
        public async Task<IActionResult> GetTracking(int idPedido)
        {
            // Usamos el nombre exacto de tu TrackingService: ObtenerSeguimientoAsync
            var tracking = await _trackingService.ObtenerSeguimientoAsync(idPedido);

            if (tracking == null)
                return NotFound(new { message = "No se encontró información de seguimiento para ese pedido." });

            return Ok(tracking);
        }
    }
}