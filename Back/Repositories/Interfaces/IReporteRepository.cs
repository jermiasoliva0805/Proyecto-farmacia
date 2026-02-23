using Back.DTOs;

namespace Back.Repositories.Interfaces
{
    public interface IReporteRepository
    {
        /// <summary>
        /// Obtiene un reporte agrupado por cadete con sus estadísticas de entrega
        /// en un rango de fechas determinado.
        /// </summary>
        Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(
            DateTime fechaDesde, 
            DateTime fechaHasta,
            int? idSucursal = null);

        Task<List<RankingClienteDTO>> GetRankingClientesFrecuentesAsync();
    // ... otros métodos existentes (como el de cadetes)
    }
}