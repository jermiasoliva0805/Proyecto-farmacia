using Back.DTOs;
using Proyecto_farmacia.DTOs;

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

        Task<List<RankingClienteDTO>> GetRankingClientesFrecuentesAsync(int dias = 7, int? idSucursal = null);
        
        /// <summary>
        /// Obtiene ranking de facturación con filtros opcionales de días y sucursal
        /// </summary>
        Task<List<ClienteFacturacionDTO>> GetRankingClientesFacturacionAsync(int dias = 7, int? idSucursal = null);

        /// <summary>
        /// Obtiene un reporte de tiempos de proceso con análisis de 4 fases críticas
        /// </summary>
        Task<TiemposProcesoDTO> GetReporteTiemposProcesoAsync(int dias = 7, int? idSucursal = null);
    }

    
}