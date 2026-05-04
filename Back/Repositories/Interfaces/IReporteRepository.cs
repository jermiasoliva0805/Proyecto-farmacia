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
            int idSucursal);

        Task<List<RankingClienteDTO>> GetRankingClientesFrecuentesAsync(int dias = 7);

        /// <summary>
        /// Obtiene ranking de facturación con filtros opcionales de días y sucursal
        /// </summary>
        Task<List<ClienteFacturacionDTO>> GetRankingClientesFacturacionAsync(int dias = 7);

        /// <summary>
        /// Obtiene el reporte de cantidad de pedidos cancelados (RF6.2)
        /// </summary>
        Task<ReportePedidosCanceladosDTO> GetReportePedidosCanceladosAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        /// <summary>
        /// Obtiene el reporte de pedidos cancelados por motivos (RF6.11)
        /// </summary>
        Task<ReporteCancelacionesPorMotivoDTO> GetReporteCancelacionesPorMotivoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        /// <summary>
        /// Obtiene el top 10 de productos más vendidos
        /// </summary>
        Task<List<TopProductosDTO>> GetTop10ProductosMasVendidosAsync(int dias = 7, int idSucursal = 1);

        /// <summary>
        /// Obtiene un reporte de tiempos de proceso con análisis de 4 fases críticas
        /// </summary>
        Task<TiemposProcesoDTO> GetReporteTiemposProcesoAsync(int dias = 7, int? idEstado = null);

        /// <summary>
        /// Obtiene el reporte de formas de pago más utilizadas (RF6.10)
        /// </summary>
        Task<ReporteFormasPagoDTO> GetReporteFormasPagoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        /// <summary>
        /// Obtiene el reporte de cantidad de pedidos por zonas con filtros opcionales
        /// </summary>
        Task<List<PedidosPorZonaDTO>> GetReportePedidosPorZonaAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idZona = null);

        /// <summary>
        /// Obtiene el reporte consolidado de la encuesta de satisfacción desde Google Forms.
        /// </summary>
        Task<ReporteEncuestaSatisfaccionDTO> GetReporteEncuestaSatisfaccionAsync();

        /// <summary>
        /// Obtiene el reporte de entregas fuera de plazo (entregadas después de FechaEntregaEstimada)
        /// </summary>
        Task<PedidosFueraDeplazoDTO> GetReportePedidosFueraDeplazoAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        /// <summary>
        /// Obtiene pedidos demorados en tiempo real (sin entregar, que ya pasaron FechaEntregaEstimada)
        /// </summary>
        Task<List<OrderSummaryDTO>> GetPedidosDemoradosAsync();

        /// <summary>
        /// Obtiene pedidos demorados filtrados según el rol del usuario logueado.
        /// - Encargado: Ve todos los pedidos demorados
        /// - Operario: Ve solo los pedidos demorados que tiene asignados
        /// - Cadete: Ve solo los pedidos demorados de su zona
        /// </summary>
        Task<List<OrderSummaryDTO>> GetPedidosDemoradosPorUsuarioAsync(int usuarioId, string rolUsuario);
    }
}
