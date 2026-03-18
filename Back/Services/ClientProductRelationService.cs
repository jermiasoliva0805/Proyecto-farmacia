using Back.Models;
using Back.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Services
{
    public class ClientProductRelationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IClientRepository _clientRepository;

        public ClientProductRelationService(IOrderRepository orderRepository, IClientRepository clientRepository)
        {
            _orderRepository = orderRepository;
            _clientRepository = clientRepository;
        }

        /// <summary>
        /// Obtiene la etiqueta logística del cliente basándose en:
        /// 1. Los productos del pedido actual (si está disponible)
        /// 2. El historial de compras del cliente
        /// </summary>
        /// <param name="clientId">ID del cliente</param>
        /// <param name="pedidoActualId">ID del pedido actual (opcional)</param>
        /// <returns>Etiqueta logística descriptiva ej: "Ramirez - Pañales Adulto"</returns>
        public async Task<string> ObtenerEtiquetaLogisticaAsync(int clientId, int? pedidoActualId = null)
        {
            try
            {
                // Obtener información del cliente
                var cliente = await _clientRepository.GetByIdAsync(clientId);
                if (cliente == null)
                    return "Cliente Desconocido";

                // Obtener la categoría principal del cliente
                var categoriasPrincipal = await ObtenerCategoriasPrincipalesAsync(clientId, pedidoActualId);

                if (string.IsNullOrEmpty(categoriasPrincipal))
                {
                    return $"{cliente.Apellido} - Consumidor General";
                }

                return $"{cliente.Apellido} - {categoriasPrincipal}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClientProductRelationService] Error al obtener etiqueta logística: {ex.Message}");
                return "Etiqueta no disponible";
            }
        }

        /// <summary>
        /// Analiza el historial de compras del cliente y retorna la(s) categoría(s) principal(es)
        /// </summary>
        private async Task<string> ObtenerCategoriasPrincipalesAsync(int clientId, int? pedidoActualId = null)
        {
            try
            {
                // Obtener todos los pedidos del cliente
                var pedidosCliente = await _orderRepository.GetClientOrdersAsync(clientId);
                
                if (pedidosCliente == null || !pedidosCliente.Any())
                    return null;

                // Diccionario para contar categorías
                var categoriasConteo = new Dictionary<string, int>();

                foreach (var pedido in pedidosCliente)
                {
                    // Si hay un pedidoActualId, incluirlo
                    if (pedidoActualId.HasValue && pedido.IDPedido == pedidoActualId)
                    {
                        AgregarCategoriasDelPedido(pedido, categoriasConteo);
                    }
                    // Si es otro pedido, también contar sus categorías (historial)
                    else if (!pedidoActualId.HasValue || pedido.IDPedido != pedidoActualId)
                    {
                        AgregarCategoriasDelPedido(pedido, categoriasConteo);
                    }
                }

                if (categoriasConteo.Count == 0)
                    return null;

                // Obtener las top categorías (máximo 2 para no saturar la etiqueta)
                var topCategorias = categoriasConteo
                    .OrderByDescending(x => x.Value)
                    .Take(2)
                    .Select(x => x.Key)
                    .ToList();

                return string.Join(" + ", topCategorias);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClientProductRelationService] Error al obtener categorías principales: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Agrega las categorías de un pedido específico al diccionario de conteo
        /// </summary>
        private void AgregarCategoriasDelPedido(Pedido pedido, Dictionary<string, int> categoriasConteo)
        {
            if (pedido?.Detalles == null || !pedido.Detalles.Any())
                return;

            var categorias = pedido.Detalles
                .Where(d => d.Producto != null && !string.IsNullOrWhiteSpace(d.Producto.Categoria))
                .Select(d => d.Producto.Categoria)
                .Distinct();

            foreach (var categoria in categorias)
            {
                if (categoriasConteo.ContainsKey(categoria))
                    categoriasConteo[categoria]++;
                else
                    categoriasConteo[categoria] = 1;
            }
        }
    }
}
