using System;
using System.Text;

namespace Back.Services
{
    public class EmailTemplateService
    {
        // Genera el HTML del correo según el estado del pedido
        public string BuildOrderStatusHtml(
            string brandName,
            string nombreCliente,
            string estadoDescripcion,   // Ej: "Entregado"
            int numeroPedido,
            int idEstado,
            string supportEmail = null,
            string brandCode = "FGP",
            int? intentoEntrega = null, // solo para estado 8 (fallido)
            int intentosMax = 3         // solo para estado 8 (fallido)
        )
        {
            var pedidoCodigo = FormatearCodigo(numeroPedido, brandCode);
            var (titulo, subtitulo, badgeText, badgeColor) = MapEstado(idEstado, estadoDescripcion, pedidoCodigo, intentoEntrega, intentosMax);

            // Paleta
            var brandBg = "#1E3A8A";    // Header azul
            var textColor = "#111827";  // Texto principal
            var panelBg = "#F9FAFB";    // Panel mensaje
            var borderColor = "#E5E7EB";
            var footerColor = "#6B7280";

            var sb = new StringBuilder();
            sb.Append($@"
<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>{titulo}</title>
</head>
<body style=""margin:0;padding:0;background-color:#f3f4f6;"">
<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#f3f4f6;"">
    <tr>
    <td align=""center"">
        <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#ffffff;margin:24px;border-radius:8px;overflow:hidden;border:1px solid {borderColor};font-family:Segoe UI, Tahoma, sans-serif;color:{textColor};"">
        <!-- Header -->
        <tr>
            <td style=""background-color:{brandBg};color:#ffffff;padding:16px 24px;font-weight:600;font-size:18px;"">
            {brandName}
            </td>
        </tr>

        <!-- Título -->
        <tr>
            <td style=""padding:24px 24px 8px;text-align:center;"">
            <h1 style=""margin:0;font-size:24px;font-weight:700;"">{titulo}</h1>
            </td>
        </tr>

        <!-- Subtítulo -->
        <tr>
            <td style=""padding:0 24px 16px;text-align:center;color:#374151;font-size:15px;"">
            {subtitulo}
            </td>
        </tr>

        <!-- Panel saludo + mensaje -->
        <tr>
            <td style=""padding:0 24px 16px;"">
            <div style=""background:{panelBg};border:1px solid {borderColor};border-radius:8px;padding:16px;font-size:15px;color:#374151;"">
                <p style=""margin:0 0 8px 0;"">Hola <strong>{nombreCliente}</strong>,</p>
                <p style=""margin:0;"">{MensajePrincipal(idEstado, pedidoCodigo, intentoEntrega, intentosMax)}</p>
            </div>
            </td>
        </tr>

        <!-- Texto adicional -->
        <tr>
            <td style=""padding:0 24px 16px;color:#4B5563;font-size:14px;"">
            {TextoAdicional(idEstado)}
            </td>
        </tr>

        <!-- Badge de estado -->
        <tr>
            <td style=""padding:8px 24px 24px;text-align:center;"">
            <span style=""display:inline-block;padding:10px 14px;border-radius:6px;background:{badgeColor};color:#ffffff;font-weight:600;font-size:14px;"">
                Estado: {badgeText}
            </span>
            </td>
        </tr>

        <!-- Footer -->
        <tr>
            <td style=""padding:16px 24px 24px;text-align:center;color:{footerColor};font-size:12px;border-top:1px solid {borderColor};"">
            <p style=""margin:8px 0;"">Si tienes alguna pregunta o inquietud sobre tu pedido, no dudes en contactarnos{(string.IsNullOrWhiteSpace(supportEmail) ? "." : $@" al <a href=""mailto:{supportEmail}"" style=""color:{brandBg};text-decoration:none;"">{supportEmail}</a>.")}</p>
            <p style=""margin:8px 0;"">© {DateTime.Now.Year} {brandName}. Todos los derechos reservados.</p>
            <p style=""margin:8px 0;"">Este es un correo automático, por favor no responder.</p>
            </td>
        </tr>
        </table>
    </td>
    </tr>
</table>
</body>
</html>");
            return sb.ToString();
        }

        private (string titulo, string subtitulo, string badgeText, string badgeColor)
            MapEstado(int idEstado, string estadoDescripcion, string pedidoCodigo, int? intentoEntrega, int intentosMax)
        {
            // Colores badge
            string green = "#10B981";   // Entregado
            string orange = "#F59E0B";  // Demorado
            string blue = "#3B82F6";    // En camino / Preparando
            string teal = "#14B8A6";    // Listo para despachar
            string indigo = "#6366F1";  // Despachando
            string red = "#EF4444";     // Entrega fallida
            string darkRed = "#DC2626"; // Cancelado
            string gray = "#6B7280";    // Recibido

            return idEstado switch
            {
                2 => ("Preparando tu Pedido", $"Estamos preparando tu pedido {pedidoCodigo}.", "Preparando tu Pedido", blue),
                3 => ("Pedido Demorado", $"Tu pedido {pedidoCodigo} está experimentando un retraso.", "Pedido Demorado", orange),
                4 => ("Pedido Listo para despachar", $"Tu pedido {pedidoCodigo} está listo para despachar.", "Pedido Listo para despachar", teal),
                5 => ("Pedido Despachado", $"Tu pedido {pedidoCodigo} ha sido despachado.", "Pedido Despachado", indigo),
                6 => ("Pedido en Camino", $"Tu pedido {pedidoCodigo} está en camino.", "Pedido en Camino", blue),
                7 => ("¡Pedido Entregado!", $"Tu pedido {pedidoCodigo} ha sido entregado exitosamente.", "Pedido Entregado", green),
                8 => ("Intento de Entrega Fallido",
                    $"No pudimos entregar tu pedido {pedidoCodigo}." +
                    (intentoEntrega.HasValue ? $" Este es el intento {intentoEntrega.Value} de {intentosMax}." : ""),
                    "Intento de Entrega Fallido",
                    red),
                10 => ("Pedido Cancelado", $"Tu pedido {pedidoCodigo} ha sido cancelado.", "Pedido Cancelado", darkRed),
                _ => ("Pedido Recibido", $"Hemos recibido tu pedido {pedidoCodigo} correctamente.", "Pedido Recibido", gray)
            };
        }

        private string MensajePrincipal(int idEstado, string pedidoCodigo, int? intentoEntrega, int intentosMax)
        {
            return idEstado switch
            {
                2 => $"Estamos preparando tu pedido {pedidoCodigo}.",
                3 => $"Tu pedido {pedidoCodigo} está experimentando un retraso.",
                4 => $"Tu pedido {pedidoCodigo} está listo para despachar.",
                5 => $"Tu pedido {pedidoCodigo} ha sido despachado.",
                6 => $"Nuestro repartidor está en ruta hacia tu domicilio. Deberías recibirlo en los próximos minutos.",
                7 => $"¡Gracias por confiar en nosotros! Esperamos que disfrutes de tus productos.",
                8 => $"No pudimos entregar tu pedido {pedidoCodigo}. " +
                    (intentoEntrega.HasValue
                        ? $"Este es el intento {intentoEntrega.Value} de {intentosMax}. Intentaremos nuevamente en las próximas horas."
                        : "Intentaremos nuevamente en las próximas horas."),
                10 => $"Tu pedido {pedidoCodigo} ha sido cancelado.",
                _ => $"Tu pedido está en cola y pronto comenzaremos a prepararlo. Te mantendremos informado sobre su progreso."
            };
        }

        private string TextoAdicional(int idEstado)
        {
            return idEstado switch
            {
                2 => "Nuestro equipo está seleccionando cuidadosamente todos los productos de tu pedido. Pronto estará listo para despachar.",
                3 => "Estamos trabajando para resolver la situación lo antes posible. Lamentamos las molestias y te mantendremos informado.",
                4 => "Tu pedido será despachado en breve. Te avisaremos cuando esté en camino.",
                5 => "Tu pedido ya salió de nuestra farmacia y está en manos de nuestro equipo de entrega. Pronto lo recibirás en tu domicilio.",
                6 => "Si tienes alguna pregunta o inquietud sobre tu pedido, no dudes en contactarnos.",
                7 => "Si tienes alguna pregunta o inquietud sobre tu pedido, no dudes en contactarnos.",
                8 => "Por favor, asegúrate de estar disponible o contáctanos para coordinar una nueva entrega.",
                10 => "Si no solicitaste esta cancelación, por favor contáctanos para revisar tu caso.",
                _ => "Te mantendremos informado sobre el progreso de tu pedido."
            };
        }

        private string FormatearCodigo(int idPedido, string brandCode)
        {
            // Formato: #FGP-YYYY-000123
            var year = DateTime.Now.Year;
            return $"#{brandCode}-{year}-{idPedido:D6}";
        }
    }
}