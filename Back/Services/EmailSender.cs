using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System;

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
            var mail = new MailMessage
            {
                From = new MailAddress(_smtpSettings.User, "Farmacia General Paz"),
                Subject = $"Estado de tu pedido #{numeroPedido}",
                Body = $"Hola {nombreCliente},\n\nEl estado de tu pedido #{numeroPedido} cambió a: {estadoPedido}.\n\n¡Gracias por confiar en nosotros!.",
                IsBodyHtml = false
            };
            mail.To.Add(destinatario);
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
            string supportEmail = null,
            string brandCode = "FGP",
            int? intentoEntrega = null,
            int intentosMax = 3,
            string trackingUrl = null,
            string etiquetaLogistica = null,
            List<string> nombresProductos = null)
        {
            var html = _templates.BuildOrderStatusHtml(
                brandName, nombreCliente, estadoDescripcion, numeroPedido, idEstado,
                supportEmail, brandCode, intentoEntrega, intentosMax, trackingUrl, etiquetaLogistica, nombresProductos);

            // Generar código personalizado para el asunto
            string codigoPersonalizado = (nombresProductos != null && nombresProductos.Count > 0)
                ? _templates.GenerarCodigoPersonalizado(nombreCliente, nombresProductos, numeroPedido)
                : $"#{numeroPedido:D6}";

            // Construir asunto con código personalizado - ESTANDARIZADO
            // Formato: "Estado del Pedido · Tu Pedido #JUANDIAZ-PAR-IBU-ASP-0002"
            var asunto = $"{estadoDescripcion} · Tu Pedido {codigoPersonalizado}";

            var mail = new MailMessage
            {
                From = new MailAddress(_smtpSettings.User, brandName),
                Subject = asunto,
                Body = html,
                IsBodyHtml = true
            };
            mail.To.Add(destinatario);

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
            List<string> nombresProductos = null)
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

                var mail = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.User, brandName),
                    Subject = $"Pedido Recibido · Tu Pedido {codigoPersonalizado}",
                    Body = html,
                    IsBodyHtml = true
                };
                mail.To.Add(destinatario);

                await Send(mail);
                Console.WriteLine($"[EmailSender] Email de tracking enviado exitosamente a {destinatario} para el pedido #{numeroPedido}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailSender] Error al enviar email de tracking: {ex.Message}");
                // No lanzar excepción para que el pedido se cree igualmente
            }
        }

        private async Task Send(MailMessage mail)
        {
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password)
            };

            try
            {
                await client.SendMailAsync(mail);
                Console.WriteLine($"[EmailSender] Email enviado a {string.Join(",", mail.To)}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"[EmailSender] Error SMTP: {ex.StatusCode} - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailSender] Error general: {ex.Message}");
                throw;
            }
        }
    }
}