using Back.DTOs;
using Back.Services.Interfaces;
using Back.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderStatusService _statusService;
        private readonly ITrackingService _trackingService;
        private readonly IPedidoRepository _pedidoRepository;

        public OrdersController(
            IOrderStatusService statusService, 
            ITrackingService trackingService, 
            IPedidoRepository pedidoRepository)
        {
            _statusService = statusService;
            _trackingService = trackingService;
            _pedidoRepository = pedidoRepository;
        }

        // --- NUEVO ENDPOINT DE REPORTES FILTRADOS ---
        // Este es el que llama tu frontend como /api/Orders/reporte
        [Authorize] // Permitir a cualquier usuario autenticado (Admin, Operario, Cadete)
        [HttpGet("reporte")]
        public async Task<IActionResult> GetReporte([FromQuery] OrderFilterDTO filters)
        {
            var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(filters);
            return Ok(pedidos);
        }

        // --- SECCIÓN ADMINISTRADOR: CONSULTAS FILTRADAS ---

        [Authorize(Roles = "Administrador")]
        [HttpGet("pendientes-operario")]
        public async Task<IActionResult> GetPendientesOperario()
        {
            var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO { IDEstadoDePedido = 1 }); 
            return Ok(pedidos);
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("pendientes-cadete")]
        public async Task<IActionResult> GetPendientesCadete()
        {
            var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO { IDEstadoDePedido = 4 }); 
            return Ok(pedidos);
        }

        // --- SECCIÓN ADMINISTRADOR: ASIGNACIÓN DE RESPONSABLES ---

        [Authorize(Roles = "Administrador")]
        [HttpPatch("asignar-operario")]
        public async Task<IActionResult> AsignarOperario([FromBody] AssignOperatorDTO dto)
        {
            if (dto == null || dto.PedidoId <= 0 || dto.OperarioId <= 0)
                return BadRequest(new { message = "Datos de asignación inválidos." });

            var resultado = await _statusService.AsignarOperarioAsync(dto);
            
            if (!resultado)
                return BadRequest(new { message = "No se pudo asignar el operario. Verifique el estado del pedido." });

            return Ok(new { message = "Operario asignado exitosamente." });
        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("asignar-cadete")]
        public async Task<IActionResult> AsignarCadete([FromBody] AssignDeliveryDTO dto)
        {
            if (dto == null || dto.PedidoId <= 0 || dto.CadeteId <= 0)
                return BadRequest(new { message = "Datos de asignación inválidos." });

            var resultado = await _statusService.AsignarCadeteAsync(dto);
            if (!resultado)
                return BadRequest(new { message = "No se pudo asignar el cadete. Verifique que el pedido esté listo." });

            return Ok(new { message = "Cadete asignado correctamente." });
        }

        // --- SECCIÓN OPERATIVA: CAMBIOS DE ESTADO ---

        [Authorize] // Permitir a Admin, Operario y Cadete cambiar estados
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] ChangeOrderStatusDTO changeStatusDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (id != changeStatusDto.IDPedido)
                return BadRequest(new { message = "El ID del pedido no coincide." });

            var resultado = await _statusService.CambiarEstadoAsync(changeStatusDto);

            if (!resultado)
                return BadRequest(new { message = "Cambio de estado rechazado por lógica de negocio." });

            // Devolver el pedido actualizado para refrescar UI sin refetch
            var pedidoActualizado = await _pedidoRepository.GetByIdAsync(id);
            return Ok(new { message = "Estado actualizado.", pedido = pedidoActualizado });
        }

        // --- SECCIÓN CONSULTAS: TRAZABILIDAD ---

        [HttpGet("{id}/seguimiento")]
        public async Task<ActionResult<OrderTrackingDTO>> GetSeguimiento(int id)
        {
            var seguimiento = await _trackingService.ObtenerSeguimientoAsync(id);
            if (seguimiento == null) return NotFound();
            return Ok(seguimiento);
        }
    }
}