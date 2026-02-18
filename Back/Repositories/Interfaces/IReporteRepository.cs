using Back.DTOs;

namespace Back.Repositories.Interfaces
{
    public interface IReporteRepository
    {
        Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(DateTime fechaDesde, DateTime fechaHasta);
    }
}
