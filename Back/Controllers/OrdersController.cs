using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;
using Back.DTOs;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/orders
        // Motivo: Recibe el DTO con el cliente, productos y cantidades para generar un nuevo pedido (RF17)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDTO orderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Usamos el método exacto de tu OrderService: CreateOrderAsync
            var orderId = await _orderService.CreateOrderAsync(orderDto);

            if (orderId <= 0)
                return BadRequest(new { message = "No se pudo procesar el pedido. Verifique el stock." });

            return Ok(new { id = orderId, message = "Pedido creado con éxito." });
        }
    }
}