using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CatalogoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCatalogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor, selecciona un archivo CSV.");

            using var stream = file.OpenReadStream();
            return await ProcesarCsv(stream);
        }

        [HttpPost("sincronizar-local")]
        public async Task<IActionResult> SincronizarLocal()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seeds", "Catalogo de Productos.xlsx - Perfumeria.csv");

            if (!System.IO.File.Exists(filePath))
                return NotFound($"No se encontró el archivo en: {filePath}");

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await ProcesarCsv(stream);
        }

        private async Task<IActionResult> ProcesarCsv(Stream stream)
        {
            try
            {
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    // ESTA LÍNEA ARREGLA EL MAPEO: 
                    // Elimina espacios y convierte a minúsculas las cabeceras del CSV para que coincidan con el modelo
                    PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", "").Replace("_", "")
                });

                var registrosCsv = csv.GetRecords<Producto>().ToList();

                if (!registrosCsv.Any())
                    return BadRequest("No se detectaron productos. Revisa el formato del archivo.");

                foreach (var item in registrosCsv)
                {
                    var productoExistente = await _context.Productos
                        .FirstOrDefaultAsync(p => p.IDProducto == item.IDProducto);

                    if (productoExistente != null)
                    {
                        _context.Entry(productoExistente).CurrentValues.SetValues(item);
                    }
                    else
                    {
                        _context.Productos.Add(item);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"Éxito: {registrosCsv.Count} productos procesados correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error técnico: {ex.Message}");
            }
        }
    }
}