using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalidadesController : ControllerBase
    {
        private readonly ILocalidadService _localidadService;

        public LocalidadesController(ILocalidadService localidadService)
        {
            _localidadService = localidadService;
        }

        // GET: api/localidades
        // Motivo: Trae la lista de localidades para que el cliente elija una al registrarse.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Usamos el estándar Async que tienen tus otros servicios
            var localidades = await _localidadService.GetAllLocalidadesAsync();
            return Ok(localidades);
        }
    }
}