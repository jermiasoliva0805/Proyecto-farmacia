using Back.DTOs;
using Back.Services.Interfaces;
using Back.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

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

        // ==========================================
        // SECCIÓN DE REPORTES (RF6.4)
        // ==========================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("reporte-tiempos-operarios")]
        public async Task<IActionResult> GetReporteTiempos()
        {
            // Importante: El repositorio debe tener este método definido
            var resultado = await _pedidoRepository.GetTiempoPromedioArmadoAsync();
            
            if (resultado == null)
                return Ok(new List<object>()); // Retorna lista vacía si no hay datos

            return Ok(resultado);
        }

        [Authorize]
        [HttpGet("reporte")]
        public async Task<IActionResult> GetReporte([FromQuery] OrderFilterDTO filters)
        {
            var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(filters);
            return Ok(pedidos);
        }

        // ==========================================
        // SECCIÓN ADMINISTRADOR: CONSULTAS FILTRADAS
        // ==========================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("pendientes-operario")]
        public async Task<IActionResult> GetPendientesOperario()
        {
        // Corregido: 'IDEstadoDePedido' (sin espacios) y el método 'GetFilteredOrdersAsync'
        var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO 
        { 
            IDEstadoDePedido = 1 
        });
        return Ok(pedidos);
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("pendientes-cadete")]
        public async Task<IActionResult> GetPendientesCadete()
        {
        // Corregido: 'IDEstadoDePedido' (sin espacios) y el método 'GetFilteredOrdersAsync'
        var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO 
        { 
            IDEstadoDePedido = 4 
        });
        return Ok(pedidos);
        }

        // ==========================================
        // SECCIÓN ADMINISTRADOR: ASIGNACIÓN
        // ==========================================

        [Authorize(Roles = "Administrador")]
        [HttpPatch("asignar-operario")]
        public async Task<IActionResult> AsignarOperario([FromBody] AssignOperatorDTO dto)
        {
            if (dto == null || dto.PedidoId <= 0 || dto.OperarioId <= 0)
                return BadRequest(new { message = "Datos de asignación inválidos." });

            var resultado = await _statusService.AsignarOperarioAsync(dto);
            if (!resultado)
                return BadRequest(new { message = "No se pudo asignar el operario." });

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
                return BadRequest(new { message = "No se pudo asignar el cadete." });

            return Ok(new { message = "Cadete asignado correctamente." });
        }

        // ==========================================
        // SECCIÓN OPERATIVA: CAMBIOS DE ESTADO
        // ==========================================

        [Authorize]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] ChangeOrderStatusDTO changeStatusDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != changeStatusDto.IDPedido) return BadRequest(new { message = "El ID no coincide." });

            var resultado = await _pedidoRepository.ActualizarEstadoPedidoAsync(changeStatusDto);
            if (!resultado) return BadRequest(new { message = "Cambio de estado rechazado." });

            var pedidoActualizado = await _pedidoRepository.GetByIdAsync(id);
            return Ok(new { message = "Estado actualizado.", pedido = pedidoActualizado });
        }

        [Authorize]
        [HttpPost("cancelar")]
        public async Task<IActionResult> CancelarPedido([FromBody] CancelarPedidoDTO dto)
        {
            var resultado = await _statusService.CancelarPedidoAsync(dto);
            if (!resultado) return BadRequest(new { message = "No se pudo cancelar el pedido." });
            return Ok(new { message = "Pedido cancelado exitosamente." });
        }

        // ==========================================
        // SECCIÓN CONSULTAS Y SEGUIMIENTO
        // ==========================================

        [HttpGet("{id}/seguimiento")]
        public async Task<ActionResult<OrderTrackingDTO>> GetSeguimiento(int id)
        {
            var seguimiento = await _trackingService.ObtenerSeguimientoAsync(id);
            if (seguimiento == null) return NotFound();
            return Ok(seguimiento);
        }

        [Authorize]
        [HttpGet("{id}/print-data")]
        public async Task<IActionResult> GetPrintData(int id)
        {
            var order = await _pedidoRepository.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Pedido no encontrado." });

            // Mapeo a DTO para impresión
            var dto = new OrderPrintDTO
            {
                IDPedido = order.IDPedido,
                Fecha = order.Fecha,
                FormaDePago = order.FormaDePago ?? "A convenir",
                Total = order.Total,
                ClienteDireccion = order.DireccionEntrega,
                MetodoEnvio = (order.IDSucursal > 0) ? "Punto de retiro" : "Envío a domicilio",
                PuntoRetiro = order.Sucursal?.Dirección ?? "N/A",
                ClienteNombre = order.Cliente != null ? $"{order.Cliente.Nombre} {order.Cliente.Apellido}" : "Cliente Final",
                Productos = order.Detalles?.Select(d => new OrderDetailItemDTO {
                    Cantidad = d.Cantidad,
                    ProductoNombre = d.Producto?.NombreProducto ?? "Producto " + d.IDProducto,
                    PrecioUnitario = d.Producto?.PrecioProducto ?? 0
                }).ToList()
            };

            return Ok(dto);
        }

    }
}

