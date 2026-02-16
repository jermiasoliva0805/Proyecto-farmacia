using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;
using Back.DTOs;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductosController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/productos
        // Motivo: Listar medicamentos y productos para el catálogo y control de stock (Mandato)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Devuelve ProductDTO mapeados desde los productos cargados del CSV
            var productos = await _productService.GetAllProductsAsync();
            return Ok(productos);
        }
    }
}
