using Back.DTOs; // Asegúrate de que apunte a donde definiste el EntregaPorCadeteDTO

namespace Back.Repositories.Interfaces
{
    public interface IReporteRepository
    {
        /// <summary>
        /// Obtiene un reporte agrupado por cadete con sus estadísticas de entrega
        /// en un rango de fechas determinado.
        /// </summary>
        Task<List<EntregaPorCadeteDTO>> GetReporteEntregasPorCadeteAsync(DateTime fechaDesde, DateTime fechaHasta);
    }
}