using Back.Models;
using Back.Interfaces;
using Back.Services.Interfaces;
using Back.DTOs;

namespace Back.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepo;

        public DeliveryService(IDeliveryRepository deliveryRepo)
        {
            _deliveryRepo = deliveryRepo;
        }

        public async Task RegistrarIntento(DeliveryAttemptDTO dto)
        {
            var intento = new IntentoDeEntrega
            {
                IDPedido = dto.PedidoId,
                FechaDeIntento = dto.FechaIntento,
                Resultado = dto.EsExitoso ? "Exitoso" : "Fallido",
                RazonDeFallo = dto.Comentario ?? "",
                URL_Foto_Verificacion = dto.RutaFotoEvidencia ?? "",
                IDUsuario = 1 // ID del cadete logueado
            };

            await _deliveryRepo.AddAttemptAsync(intento);
        }
    }
}