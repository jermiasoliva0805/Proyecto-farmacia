namespace Back.DTOs
{
    public class EntregaPorCadeteDTO
    {
        public int IDCadete { get; set; }
        public string NombreCadete { get; set; } = string.Empty;
        public int TotalPedidosAsignados { get; set; }
        public int EntregasExitosas { get; set; }
        public int EntregasFallidas { get; set; }
        public decimal TotalRecaudado { get; set; }
        public double PorcentajeEfectividad => TotalPedidosAsignados > 0 
            ? (double)EntregasExitosas / TotalPedidosAsignados * 100 
            : 0;
    }
}