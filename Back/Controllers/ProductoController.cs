using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;
using Back.DTOs;
using AutoMapper;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductosController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        // GET: api/productos
        // Motivo: Listar medicamentos y productos para el catálogo y control de stock (Mandato)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Devuelve ProductDTO mapeados desde los productos cargados del CSV
            var productos = await _productService.GetAllProductsAsync();
            var productosDTO = _mapper.Map<IEnumerable<ProductDTO>>(productos);
            return Ok(productosDTO);
        }
    }
}
