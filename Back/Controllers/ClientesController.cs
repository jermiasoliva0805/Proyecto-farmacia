using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientesController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // GET: api/clientes
        // Motivo: Listar clientes para los combos del frontend (RF5)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Usamos el nombre exacto de tu Service: GetAllClientsAsync
            var clientes = await _clientService.GetAllClientsAsync();
            return Ok(clientes);
        }
    }
}