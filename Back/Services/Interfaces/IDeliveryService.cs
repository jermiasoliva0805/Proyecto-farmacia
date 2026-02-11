using Back.DTOs;

namespace Back.Services.Interfaces
{
    public interface IDeliveryService
    {
        Task RegistrarIntento(DeliveryAttemptDTO dto);
    }
}