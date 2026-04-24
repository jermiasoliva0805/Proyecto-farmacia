namespace Back.Utils
{
    /// <summary>
    /// Utilidad para cálculos de fechas y horas, especialmente para horas hábiles
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Calcula la fecha de entrega estimada sumando 48 horas hábiles (lunes a viernes)
        /// </summary>
        /// <param name="fechaCreacion">Fecha de creación del pedido</param>
        /// <returns>Fecha estimada de entrega (48 horas hábiles después)</returns>
        public static DateTime CalcularFechaEntregaEstimada(DateTime fechaCreacion)
        {
            DateTime resultado = fechaCreacion;
            int horasHabilesAgregadas = 0;
            const int HORAS_HABILES_PROMESA = 48; // 48 horas hábiles

            while (horasHabilesAgregadas < HORAS_HABILES_PROMESA)
            {
                resultado = resultado.AddHours(1);

                // Contar solo horas de lunes a viernes (0=domingo, 6=sábado)
                if (resultado.DayOfWeek != DayOfWeek.Saturday && resultado.DayOfWeek != DayOfWeek.Sunday)
                {
                    horasHabilesAgregadas++;
                }
            }

            return resultado;
        }

        /// <summary>
        /// Calcula la diferencia en horas hábiles entre dos fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial</param>
        /// <param name="fechaFin">Fecha final</param>
        /// <returns>Diferencia en horas hábiles (puede ser negativa si fin es antes que inicio)</returns>
        public static int CalcularHorasHabilesDiferencia(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
            {
                return CalcularHorasHabilesDiferencia(fechaFin, fechaInicio) * -1;
            }

            DateTime fecha = fechaInicio;
            int horasHabiles = 0;

            while (fecha < fechaFin)
            {
                if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
                {
                    horasHabiles++;
                }
                fecha = fecha.AddHours(1);
            }

            return horasHabiles;
        }

        /// <summary>
        /// Calcula la diferencia en días hábiles entre dos fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial</param>
        /// <param name="fechaFin">Fecha final</param>
        /// <returns>Diferencia en días hábiles (puede ser decimal para incluir horas)</returns>
        public static double CalcularDiasHabilesDiferencia(DateTime fechaInicio, DateTime fechaFin)
        {
            int horasHabiles = CalcularHorasHabilesDiferencia(fechaInicio, fechaFin);
            return Math.Round(horasHabiles / 24.0, 2);
        }

        /// <summary>
        /// Verifica si una fecha es un día hábil (lunes a viernes)
        /// </summary>
        public static bool EsDiaHabil(DateTime fecha)
        {
            return fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// Determina si un pedido ya excedió su fecha estimada y sigue activo.
        /// </summary>
        public static bool EsPedidoDemorado(DateTime fechaEntregaEstimada, int estadoPedido, DateTime? ahora = null)
        {
            if (fechaEntregaEstimada == DateTime.MinValue)
                return false;

            if (estadoPedido == 7 || estadoPedido == 9 || estadoPedido == 10)
                return false;

            return (ahora ?? DateTime.Now) > fechaEntregaEstimada;
        }
    }
}
