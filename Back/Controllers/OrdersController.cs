using Back.DTOs;
using Back.Services.Interfaces;
using Back.Repositories.Interfaces;
using Back.Data;
using Back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderStatusService _statusService;
        private readonly ITrackingService _trackingService;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ICancellationService _cancellationService;
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;

        public OrdersController(
            IOrderStatusService statusService,
            ITrackingService trackingService,
            IPedidoRepository pedidoRepository,
            ICancellationService cancellationService,
            IOrderService orderService,
            AppDbContext context)
        {
            _statusService = statusService;
            _trackingService = trackingService;
            _pedidoRepository = pedidoRepository;
            _cancellationService = cancellationService;
            _orderService = orderService;
            _context = context;
        }

        // ==========================================
        // CREACIÓN DE PEDIDOS
        // ==========================================

        /// <summary>
        /// Crea un nuevo pedido con sus detalles
        /// </summary>
        [Authorize(Roles = "Encargado")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO orderDto)
        {
            try
            {
                if (orderDto == null)
                    return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío." });

                if (orderDto.Detalles == null || !orderDto.Detalles.Any())
                    return BadRequest(new { message = "El pedido debe incluir al menos un producto." });

                var pedidoId = await _orderService.CreateOrderAsync(orderDto);
                return Ok(new { pedidoId = pedidoId, message = "Pedido creado exitosamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear el pedido: {ex.Message}" });
            }
        }

        // ==========================================
        // SECCIÓN DE REPORTES (UNIFICADA)
        // ==========================================

        // Reporte 1: Rendimiento de Operarios (Tiempos de Armado RF6.4)
        // Ahora acepta parámetros opcionales por QueryString
        [HttpGet("reporte-tiempos-operarios")]
        public async Task<IActionResult> GetReporteTiempos([FromQuery] int dias = 7, [FromQuery] int? idSucursal = null)
        {
            Console.WriteLine($"[CONTROLLER] GetReporteTiempos - Dias: {dias}, IdSucursal: {idSucursal}");
            
            // Pasamos los filtros al repositorio
            var resultado = await _pedidoRepository.GetTiempoPromedioArmadoAsync(dias, idSucursal);
            
            Console.WriteLine($"[CONTROLLER] Resultado: {resultado?.Count ?? 0} operarios");
            
            if (resultado == null)
                return Ok(new List<ReporteOperarioDTO>());

            return Ok(resultado);
        }

        // Reporte 2: Consulta General de Pedidos Filtrados
        [Authorize]
        [HttpGet("reporte")]
        public async Task<IActionResult> GetReporte([FromQuery] OrderFilterDTO filters)
        {
            var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(filters);
            return Ok(pedidos);
        }

        // ==========================================
        // SECCIÓN ENCARGADO: CONSULTAS FILTRADAS
        // ==========================================

        [Authorize(Roles = "Encargado")]
        [HttpGet("pendientes-operario")]
        public async Task<IActionResult> GetPendientesOperario()
        {
            var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO 
            { 
                IDEstadoDePedido = 1 
            });
            return Ok(pedidos);
        }

        [Authorize(Roles = "Encargado")]
        [HttpGet("pendientes-cadete")]
        public async Task<IActionResult> GetPendientesCadete()
        {
            var pedidos = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO 
            { 
                IDEstadoDePedido = 4 
            });
            return Ok(pedidos);
        }

        // ==========================================
        // SECCIÓN ENCARGADO: ASIGNACIÓN
        // ==========================================

        [Authorize]
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

        [Authorize]
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

        // ==========================================
        // SECCIÓN: FILTRADO DE CADETES POR ZONA
        // ==========================================

        /// <summary>
        /// Obtiene todos los cadetes disponibles (activos) para una zona específica.
        /// Filtra por la zona del pedido y cadetes que estén activos/disponibles.
        /// </summary>
        [Authorize]
        [HttpGet("cadetes-disponibles/{pedidoId}")]
        public IActionResult GetCadetesDisponibles(int pedidoId)
        {
            try
            {
                // 1. Obtener el pedido y su zona
                var pedido = _context.Pedidos.FirstOrDefault(p => p.IDPedido == pedidoId);
                if (pedido == null)
                    return NotFound(new { message = $"El pedido con ID {pedidoId} no fue encontrado." });

                // 2. Si el pedido no tiene zona asignada, devolver error
                if (pedido.ZonaId == null)
                    return BadRequest(new { message = "El pedido no tiene una zona de reparto asignada." });

                // 3. Filtrar cadetes: que sean de la misma zona, estén activos y sean cadetes
                var cadetesAptos = _context.Usuarios
                    .Where(u => u.ZonaId == pedido.ZonaId && 
                               u.IsDeleted == false &&
                               (u.Rol == "Cadete" || u.Rol == "cadete")) // Flexible con mayúsculas/minúsculas
                    .Select(u => new
                    {
                        u.IDUsuario,
                        u.Nombre,
                        u.Apellido,
                        u.UsuarioNombre,
                        u.Rol,
                        u.Mail,
                        u.ZonaId,
                        RequiereAsignacionZona = u.ZonaId == null
                    })
                    .ToList();

                if (!cadetesAptos.Any())
                    return Ok(new List<object>()); // DevolvemosLista vacía si no hay cadetes disponibles

                return Ok(cadetesAptos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener cadetes: {ex.Message}" });
            }
        }

        // ==========================================
        // DEBUG: VERIFICACIÓN DE ZONAS
        // ==========================================

        [HttpGet("debug-zonas")]
        public async Task<IActionResult> DebugZonas()
        {
            Console.WriteLine("\n[DEBUG-ZONAS] INICIO");

            var pedidos = await _context.Pedidos
                .Include(p => p.Zona)
                .Take(20)
                .ToListAsync();

            Console.WriteLine($"[DEBUG-ZONAS] Total pedidos cargados: {pedidos.Count}");

            var resultado = pedidos.Select(p => new
            {
                p.IDPedido,
                p.ZonaId,
                ZonaNombre = p.Zona?.Nombre ?? "NULL",
                ZonaId_navegacion = p.Zona?.Id ?? -1
            }).ToList();

            foreach (var r in resultado)
            {
                Console.WriteLine($"  ├─ Pedido #{r.IDPedido}: ZonaId={r.ZonaId}, Zona.Nombre={r.ZonaNombre}, Zona.Id={r.ZonaId_navegacion}");
            }

            Console.WriteLine("[DEBUG-ZONAS] FIN\n");

            return Ok(resultado);
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

            var resultado = await _statusService.CambiarEstadoAsync(changeStatusDto);
            if (!resultado) return BadRequest(new { message = "Cambio de estado rechazado por lógica de negocio." });

            var pedidoActualizado = await _pedidoRepository.GetByIdAsync(id);
            return Ok(new { message = "Estado actualizado correctamente.", pedido = pedidoActualizado });
        }

        [Authorize]
        [HttpPost("cancelar")]
        public async Task<IActionResult> CancelarPedido([FromBody] CancelarPedidoDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío." });

                if (dto.PedidoId <= 0)
                    return BadRequest(new { message = "Pedido ID inválido." });

                if (dto.MotivoCancelacionId <= 0)
                    return BadRequest(new { message = "Debe seleccionar un motivo de cancelación." });

                if (string.IsNullOrEmpty(dto.UsuarioId))
                    return BadRequest(new { message = "Usuario ID no identificado." });

                var (success, message) = await _cancellationService.CancelarPedidoAsync(dto, dto.UsuarioId);

                if (!success)
                    return BadRequest(new { message = message });

                return Ok(new { message = message, success = true });
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error en CancelarPedido: {ex}");
                return BadRequest(new { message = $"Error al procesar la cancelación: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene todos los motivos de cancelación disponibles
        /// </summary>
        [HttpGet("motivos-cancelacion")]
        public async Task<IActionResult> GetMotivosCancelacion()
        {
            var motivos = await _cancellationService.ObtenerMotivosCancelacionAsync();
            return Ok(motivos);
        }

        /// <summary>
        /// Valida si un pedido puede ser cancelado
        /// </summary>
        [Authorize]
        [HttpGet("{id}/puede-cancelarse")]
        public async Task<IActionResult> ValidarCancelacion(int id)
        {
            var (canCancel, reason) = await _cancellationService.ValidarCancelacionAsync(id);
            return Ok(new { canCancel = canCancel, reason = reason });
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

            var dto = new OrderPrintDTO
            {
                IDPedido = order.IDPedido,
                Fecha = order.Fecha,
                FormaDePago = order.FormaDePago ?? "A convenir",
                Total = order.Total,
                ClienteDireccion = order.DireccionEntrega,
                MetodoEnvio = (order.IDSucursal > 0) ? "Punto de retiro" : "Envío a domicilio",
                PuntoRetiro = order.Sucursal?.Dirección ?? "N/A"
            };

            if (order.Cliente != null)
            {
                dto.ClienteNombre = $"{order.Cliente.Nombre} {order.Cliente.Apellido}";
                dto.ClienteDNI = order.Cliente.DNI ?? "N/A";
                dto.ClienteTelefono = order.Cliente.Telefono ?? "N/A";
                dto.ClienteEmail = order.Cliente.Mail ?? "N/A";

                var barrio = order.Cliente.Barrio?.Nombre ?? "N/A";
                var localidad = order.Cliente.Localidad?.Ciudad ?? "N/A";
                dto.ClienteLocalidadBarrio = $"{barrio}, {localidad}";
            }

            if (order.Detalles != null)
            {
                dto.Productos = order.Detalles.Select(d => new OrderDetailItemDTO
                {
                    Cantidad = d.Cantidad,
                    ProductoNombre = d.Producto?.NombreProducto ?? "Producto " + d.IDProducto,
                    SKU = d.IDProducto.ToString(),
                    PrecioUnitario = d.Producto?.PrecioProducto ?? 0
                }).ToList();
            }

            return Ok(dto);
        }

        /// <summary>
        /// Obtiene los datos para la etiqueta de envío (sin productos)
        /// </summary>
        [Authorize]
        [HttpGet("{id}/label-data")]
        public async Task<IActionResult> GetLabelData(int id)
        {
            var order = await _pedidoRepository.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Pedido no encontrado." });

            var dto = new OrderLabelDTO
            {
                IDPedido = order.IDPedido,
                Fecha = order.Fecha,
                ClienteDireccion = order.DireccionEntrega,
                CodigoPostal = order.CodigoPostalEntrega,
                ReferenciaEntrega = order.ReferenciaEntrega,
                MetodoEnvio = (order.IDSucursal > 0) ? "Punto de retiro" : "Envío a domicilio",
                PuntoRetiro = order.Sucursal?.Dirección ?? "N/A"
            };

            if (order.Cliente != null)
            {
                dto.ClienteNombre = $"{order.Cliente.Nombre} {order.Cliente.Apellido}";
                dto.ClienteTelefono = order.Cliente.Telefono ?? "N/A";
                dto.ClienteEmail = order.Cliente.Mail ?? "N/A";

                var barrio = order.Cliente.Barrio?.Nombre ?? "N/A";
                var localidad = order.Cliente.Localidad?.Ciudad ?? "N/A";
                dto.ClienteLocalidadBarrio = $"{barrio}, {localidad}";
            }

            return Ok(dto);
        }
    }
}
