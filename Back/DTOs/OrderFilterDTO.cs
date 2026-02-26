using System;
using Microsoft.AspNetCore.Mvc;

namespace Back.DTOs
{
    /// <summary>
    /// Objeto de transferencia para los criterios de búsqueda (RF13)
    /// </summary>
    public class OrderFilterDTO
    {
        [FromQuery(Name = "idEstadoDePedido")]
        public int? IDEstadoDePedido { get; set; }

        [FromQuery(Name = "idUsuario")]
        public int? IDUsuario { get; set; }

        [FromQuery(Name = "idCliente")]
        public int? IDCliente { get; set; }

        [FromQuery(Name = "fechaDesde")]
        public DateTime? FechaDesde { get; set; }

        [FromQuery(Name = "fechaHasta")]
        public DateTime? FechaHasta { get; set; }

        [FromQuery(Name = "search")]
        public string? Search { get; set; }
    }
}