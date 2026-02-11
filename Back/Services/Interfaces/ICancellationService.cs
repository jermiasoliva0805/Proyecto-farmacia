namespace Back.Services.Interfaces
{
    public interface ICancellationService
    {
        Task CancelarPedido(int idPedido, string motivo, int idUsuario);
    }
}