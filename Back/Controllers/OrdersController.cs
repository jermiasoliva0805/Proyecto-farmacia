using Back.Data;
using Back.DTOs;
using Back.Models;
using Back.Repositories.Interfaces;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _pedidoRepository;
        private readonly IOrderStatusService _statusService;

        public OrdersController(
            AppDbContext context,
            IOrderRepository pedidoRepository,
            IOrderStatusService statusService)
        {
            _context = context;
            _pedidoRepository = pedidoRepository;
            _statusService = statusService;
        }

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

        [Authorize]
        [HttpPost("crear")]
        public async Task<IActionResult> CrearPedido([FromBody] CreateOrderDTO orderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Implementar la lógica de creación aquí
                return Ok(new { message = "Pedido creado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpGet("cadetes-disponibles/{pedidoId}")]
        public IActionResult GetCadetesDisponibles(int pedidoId)
        {
            try
            {
                var pedido = _context.Pedidos.FirstOrDefault(p => p.IDPedido == pedidoId);
                if (pedido == null)
                    return NotFound(new { message = $"El pedido con ID {pedidoId} no fue encontrado." });

                if (pedido.ZonaId == null)
                    return BadRequest(new { message = "El pedido no tiene una zona de reparto asignada." });

                var cadetesAptos = _context.Usuarios
                    .Where(u => u.ZonaId == pedido.ZonaId && 
                               u.IsDeleted == false &&
                               (u.Rol == "Cadete" || u.Rol == "cadete"))
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
                    return Ok(new List<object>());

                return Ok(cadetesAptos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener cadetes: {ex.Message}" });
            }
        }

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
        public async Task<IActionResult> CancelarPedido([FromBody] ChangeOrderStatusDTO changeStatusDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = await _statusService.CancelarPedidoAsync(changeStatusDto);
            if (!resultado) return BadRequest(new { message = "No se pudo cancelar el pedido." });

            return Ok(new { message = "Pedido cancelado correctamente." });
        }
    }
}
