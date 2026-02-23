
namespace Back.DTOs
{
    public class RankingClienteDTO
    {
        public string NombreCliente { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal GastoTotal { get; set; }
        public decimal TicketPromedio { get; set; }
        public DateTime UltimaCompra { get; set; }
    }
}
