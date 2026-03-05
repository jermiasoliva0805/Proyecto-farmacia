using Microsoft.AspNetCore.Mvc;
using Back.Repositories.Interfaces;
using Back.DTOs;
using AutoMapper;
using Back.Models;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SucursalesController : ControllerBase
    {
        private readonly IGenericRepository<Sucursal> _sucursalRepository;
        private readonly IMapper _mapper;

        public SucursalesController(IGenericRepository<Sucursal> sucursalRepository, IMapper mapper)
        {
            _sucursalRepository = sucursalRepository;
            _mapper = mapper;
        }

        // GET: api/sucursales
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sucursales = await _sucursalRepository.GetAllAsync();
            var sucursalesDTO = _mapper.Map<IEnumerable<SucursalDTO>>(sucursales);
            return Ok(sucursalesDTO);
        }
    }
}
