using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;
using Back.Data;
using Back.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                Console.WriteLine("📍 Iniciando GetAll localidades...");
                
                // Intentar usar el servicio
                IEnumerable<Localidad> localidades = null;
                try
                {
                    localidades = await _localidadservice.GetAllLocalidadesAsync();
                    Console.WriteLine($"✅ Servicio obtuvo {localidades?.Count()} localidades");
                }
                catch (Exception serviceEx)
                {
                    Console.WriteLine($"⚠️ Error en servicio ({serviceEx.Message}), usando DbContext directo...");
                    localidades = await _context.Localidades.ToListAsync();
                    Console.WriteLine($"✅ DbContext obtuvo {localidades?.Count()} localidades");
                }
                
                // Mapear a DTO con camelCase para el frontend
                var localidadesDTO = (localidades ?? Enumerable.Empty<Localidad>())
                    .Select(l => new
                    {
                        idLocalidad = l.IDLocalidad,
                        ciudad = l.Ciudad,
                        provincia = l.Provincia,
                        codigoPostal = l.CodigoPostal
                    })
                    .ToList();
                
                Console.WriteLine($"✅ Devolviendo {localidadesDTO.Count} localidades");
                return Ok(localidadesDTO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error crítico en GetAll: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    message = $"Error al obtener localidades: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }

        // GET: api/localidades/zonas
        // Obtiene todas las zonas de reparto disponibles
        [HttpGet("zonas")]
        public IActionResult GetZonas()
        {
            try
            {
                Console.WriteLine("📍 [GetZonas] Iniciando...");
                
                if (_context.Zonas == null)
                {
                    Console.WriteLine("❌ [GetZonas] DbContext.Zonas es null");
                    return Ok(new List<object>());
                }

                var totalZonas = _context.Zonas.Count();
                Console.WriteLine($"📊 [GetZonas] Total de zonas en BD: {totalZonas}");

                var zonas = _context.Zonas
                    .AsNoTracking()
                    .ToList(); // Cargar en memoria

                Console.WriteLine($"📋 [GetZonas] Zonas cargadas: {zonas.Count}");
                foreach (var z in zonas)
                {
                    Console.WriteLine($"   - Zona {z.Id}: {z.Nombre}");
                }

                var zonasDto = zonas
                    .Select(z => new { id = z.Id, nombre = z.Nombre })
                    .ToList();

                Console.WriteLine($"✅ [GetZonas] Devolviendo {zonasDto.Count} zonas");
                return Ok(zonasDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GetZonas] Error: {ex.Message}");
                Console.WriteLine($"❌ [GetZonas] Stack: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Error al obtener zonas: {ex.Message}", stackTrace = ex.StackTrace });
            }
        }

        // GET: api/localidades/zonas-debug
        // SOLO PARA DEBUG: Muestra todas las zonas con sus barrios
        [HttpGet("zonas-debug")]
        public IActionResult GetZonasDebug()
        {
            try
            {
                var zonas = _context.Zonas
                    .AsNoTracking()
                    .Include(z => z.Barrios)
                    .ToList();

                var resultado = zonas.Select(z => new
                {
                    id = z.Id,
                    nombre = z.Nombre,
                    barrios = z.Barrios.Select(b => new
                    {
                        idBarrio = b.IDBarrio,
                        nombreBarrio = b.Nombre,
                        idLocalidad = b.IDLocalidad
                    }).ToList(),
                    totalBarrios = z.Barrios.Count
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/localidades/zonas-pedidos
        // SOLO PARA DEBUG: Muestra todas las zonas con sus pedidos
        [HttpGet("zonas-pedidos")]
        public IActionResult GetZonasPedidos()
        {
            try
            {
                var zonas = _context.Zonas
                    .AsNoTracking()
                    .Include(z => z.Pedidos)
                    .ToList();

                var resultado = zonas.Select(z => new
                {
                    id = z.Id,
                    nombre = z.Nombre,
                    totalPedidos = z.Pedidos.Count,
                    pedidos = z.Pedidos.Select(p => new
                    {
                        idPedido = p.IDPedido,
                        estado = p.EstadoActual,
                        fecha = p.Fecha,
                        total = p.Total
                    }).ToList()
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
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