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
            try
            {
                // Usamos el método definido en tu interfaz ILocalidadService
                var localidades = await _localidadservice.GetAllLocalidadesAsync();
                
                // Mapear a DTO con camelCase para el frontend
                var localidadesDTO = localidades.Select(l => new
                {
                    idLocalidad = l.IDLocalidad,
                    ciudad = l.Ciudad,
                    provincia = l.Provincia,
                    codigoPostal = l.CodigoPostal
                }).ToList();
                
                Console.WriteLine($"✅ Localidades devueltas: {localidadesDTO.Count}");
                return Ok(localidadesDTO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetAll localidades: {ex.Message}");
                return StatusCode(500, new { message = $"Error al obtener localidades: {ex.Message}" });
            }
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
                        idBarrio = b.IDBarrio,
                        nombre = b.Nombre,
                        zonaId = b.ZonaId,
                        zonaNombre = b.Zona != null ? b.Zona.Nombre : null
                    })
                    .ToList();

                // Devolver array vacío en lugar de 404 (mejor UX)
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
                        idBarrio = b.IDBarrio,
                        nombre = b.Nombre,
                        zonaId = b.ZonaId,
                        zonaNombre = b.Zona != null ? b.Zona.Nombre : null
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