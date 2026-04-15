using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;
using Back.Data;
using Back.Models;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalidadesController : ControllerBase
    {
        private readonly ILocalidadService _localidadservice;
        private readonly AppDbContext _context;

        public LocalidadesController(ILocalidadService localidadService, AppDbContext context)
        {
            _localidadservice = localidadService;
            _context = context;
        }

        // GET: api/localidades
        // Motivo: Cargar el listado de localidades para formularios (RF6)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Usamos el método definido en tu interfaz ILocationService
            var localidades = await _localidadservice.GetAllLocalidadesAsync();
            return Ok(localidades);
        }

        // GET: api/localidades/zonas
        // Obtiene todas las zonas de reparto disponibles
        [HttpGet("zonas")]
        public IActionResult GetZonas()
        {
            try
            {
                var zonas = _context.Zonas
                    .Select(z => new { z.Id, z.Nombre })
                    .ToList();

                return Ok(zonas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener zonas: {ex.Message}" });
            }
        }

        // GET: api/localidades/{localidadId}/barrios
        // Obtiene todos los barrios de una localidad con su zona asignada
        [HttpGet("{localidadId}/barrios")]
        public IActionResult GetBarriosPorLocalidad(int localidadId)
        {
            try
            {
                var barrios = _context.Barrios
                    .Where(b => b.IDLocalidad == localidadId)
                    .Select(b => new
                    {
                        b.IDBarrio,
                        b.Nombre,
                        b.ZonaId,
                        ZonaNombre = b.Zona != null ? b.Zona.Nombre : null
                    })
                    .ToList();

                if (!barrios.Any())
                    return NotFound(new { message = $"No hay barrios para la localidad {localidadId}" });

                return Ok(barrios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener barrios: {ex.Message}" });
            }
        }

        // GET: api/localidades/barrio/{barrioId}/zona
        // Obtiene la zona correspondiente a un barrio
        [HttpGet("barrio/{barrioId}/zona")]
        public IActionResult GetZonaPorBarrio(int barrioId)
        {
            try
            {
                var barrio = _context.Barrios
                    .Where(b => b.IDBarrio == barrioId)
                    .Select(b => new
                    {
                        b.IDBarrio,
                        b.Nombre,
                        b.ZonaId,
                        ZonaNombre = b.Zona != null ? b.Zona.Nombre : null
                    })
                    .FirstOrDefault();

                if (barrio == null)
                    return NotFound(new { message = $"Barrio {barrioId} no encontrado" });

                return Ok(barrio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener zona del barrio: {ex.Message}" });
            }
        }
    }
}