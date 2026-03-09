using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;
using Back.DTOs;
using AutoMapper;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IMapper _mapper;

        public ClientesController(IClientService clientService, IMapper mapper)
        {
            _clientService = clientService;
            _mapper = mapper;
        }

        // GET: api/clientes
        // Motivo: Listar clientes para los combos del frontend (RF5)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Usamos el nombre exacto de tu Service: GetAllClientsAsync
            var clientes = await _clientService.GetAllClientsAsync();
            var clientesDto = _mapper.Map<IEnumerable<ClientDTO>>(clientes);
            return Ok(clientesDto);
        }
    }
}