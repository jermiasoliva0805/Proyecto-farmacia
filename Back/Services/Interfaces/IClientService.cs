using Back.Models;

namespace Back.Services.Interfaces
{
    public interface IClientService
    {
        // Para listar clientes en el combo del frontend
        Task<IEnumerable<Cliente>> GetAllClientsAsync();
        // Para obtener los datos del cliente seleccionado (ej. su barrio o dirección)
        Task<Cliente> GetClientByIdAsync(int id);
    }

    public interface IProductService
    {
        // Para listar productos y ver stock (Mandato)
        Task<IEnumerable<Producto>> GetAllProductsAsync();
        // Para validar stock y precio unitario al momento de cargar un pedido
        Task<Producto> GetProductByIdAsync(int id);
    }


}