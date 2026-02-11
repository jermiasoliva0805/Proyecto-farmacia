using Back.Data;
using Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotivosCancelacionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MotivosCancelacionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MotivosCancelacion
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MotivoCancelacion>>> GetMotivos()
        {
            // Retornamos la lista de motivos para llenar el combo en el frontend
            return await _context.MotivosCancelacion
                .OrderBy(m => m.Nombre) // Suponiendo que tiene una propiedad Nombre o Descripcion
                .ToListAsync();
        }
    }
}