using Back.Models;

namespace Back.Interfaces
{
    public interface ICancellationRepository
    {
        Task CancelOrderAsync(int orderId, string reason, string userId);
    }
}
