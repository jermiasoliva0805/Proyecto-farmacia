using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Back.Services
{
    public class EmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly EmailTemplateService _templates;

        public EmailSender(IOptions<SmtpSettings> smtpOptions, EmailTemplateService templates)
        {
            _smtpSettings = smtpOptions.Value;
            _templates = templates;
        }

        // Texto plano (compatibilidad)
        public async Task EnviarCorreoCambioEstado(string destinatario, string nombreCliente, string estadoPedido, int numeroPedido)
        {
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("Farmacia General Paz", _smtpSettings.User));
            mail.To.Add(MailboxAddress.Parse(destinatario));
            mail.Subject = $"Estado de tu pedido #{numeroPedido}";
            mail.Body = new TextPart("plain")
            {
                Text = $"Hola {nombreCliente},\n\nEl estado de tu pedido #{numeroPedido} cambió a: {estadoPedido}.\n\n¡Gracias por confiar en nosotros!."
            };
            await Send(mail);
        }

        // HTML personalizado
        public async Task EnviarCorreoCambioEstadoHtml(
            string destinatario,
            string nombreCliente,
            string estadoDescripcion,
            int numeroPedido,
            int idEstado,
            string brandName = "Farmacia General Paz",
            string? supportEmail = null,
            string brandCode = "FGP",
            int? intentoEntrega = null,
            int intentosMax = 3,
            string? trackingUrl = null,
            string? etiquetaLogistica = null,
            List<string>? nombresProductos = null,
            string? surveyUrl = null)
        {
            var html = _templates.BuildOrderStatusHtml(
                brandName, nombreCliente, estadoDescripcion, numeroPedido, idEstado,
                supportEmail, brandCode, intentoEntrega, intentosMax, trackingUrl, etiquetaLogistica, nombresProductos, surveyUrl);

            // Generar código personalizado para el asunto
            string codigoPersonalizado = (nombresProductos != null && nombresProductos.Count > 0)
                ? _templates.GenerarCodigoPersonalizado(nombreCliente, nombresProductos, numeroPedido)
                : $"#{numeroPedido:D6}";

            // Construir asunto con código personalizado - ESTANDARIZADO
            // Formato: "Estado del Pedido · Tu Pedido #JUANDIAZ-PAR-IBU-ASP-0002"
            var asunto = $"{estadoDescripcion} · Tu Pedido {codigoPersonalizado}";

            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress(brandName, _smtpSettings.User));
            mail.To.Add(MailboxAddress.Parse(destinatario));
            mail.Subject = asunto;
            mail.Body = new TextPart("html") { Text = html };

            await Send(mail);
        }

        /// <summary>
        /// Envía el email de confirmación de pedido con tracking link
        /// </summary>
        public async Task EnviarCorreoTrackingAsync(
            string destinatario,
            string nombreCliente,
            int numeroPedido,
            string trackingUrl,
            string brandName = "Farmacia General Paz",
            string supportEmail = "soporte@farmacia.com",
            string brandCode = "FGP",
            List<string>? nombresProductos = null)
        {
            try
            {
                var html = _templates.BuildOrderTrackingWelcomeHtml(
                    brandName,
                    nombreCliente,
                    numeroPedido,
                    trackingUrl,
                    brandCode,
                    supportEmail,
                    nombresProductos);

                // Generar código personalizado para el asunto
                string codigoPersonalizado = (nombresProductos != null && nombresProductos.Count > 0)
                    ? _templates.GenerarCodigoPersonalizado(nombreCliente, nombresProductos, numeroPedido)
                    : $"#{numeroPedido:D6}";

                var mail = new MimeMessage();
                mail.From.Add(new MailboxAddress(brandName, _smtpSettings.User));
                mail.To.Add(MailboxAddress.Parse(destinatario));
                mail.Subject = $"Pedido Recibido · Tu Pedido {codigoPersonalizado}";
                mail.Body = new TextPart("html") { Text = html };

                await Send(mail);
                Console.WriteLine($"[EmailSender] Email de tracking enviado exitosamente a {destinatario} para el pedido #{numeroPedido}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailSender] Error al enviar email de tracking: {ex.Message}");
                // No lanzar excepción para que el pedido se cree igualmente
            }
        }

        private async Task Send(MimeMessage mail)
        {
            // Validar configuración SMTP
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host))
            {
                Console.WriteLine($"[EmailSender] ⚠️ ADVERTENCIA: SMTP Host no configurado. Email NO se envió a {string.Join(",", mail.To)}");
                return;
            }

            if (string.IsNullOrWhiteSpace(_smtpSettings.User) || string.IsNullOrWhiteSpace(_smtpSettings.Password))
            {
                Console.WriteLine($"[EmailSender] ⚠️ ADVERTENCIA: Credenciales SMTP incompletas. Email NO se envió a {string.Join(",", mail.To)}");
                return;
            }

            try
            {
                Console.WriteLine($"[EmailSender] Enviando email a {string.Join(",", mail.To)} via {_smtpSettings.Host}:{_smtpSettings.Port}");

                using var client = new SmtpClient();
                // SecureSocketOptions.StartTls fuerza STARTTLS en el puerto 587
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.User, _smtpSettings.Password);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);

                Console.WriteLine($"[EmailSender] ✅ Email enviado exitosamente a {string.Join(",", mail.To)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailSender] ❌ Error al enviar email: {ex.Message}");
                throw;
            }
        }
    }
}