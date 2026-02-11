using Back.Models;
using Back.Repositories.Interfaces;
using Back.Services.Interfaces;

namespace Back.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Producto> _productRepository;

        public ProductService(IGenericRepository<Producto> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Producto>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }
        /// <summary>
        /// Obtiene un producto específico por su ID.
        /// Se utiliza para validar stock y obtener el precio unitario real 
        /// al momento de crear o editar un pedido.
        /// </summary>
        public async Task<Producto> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }
    }
}