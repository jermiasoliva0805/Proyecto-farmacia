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
            string etiquetaLogistica = null)
        {
            var html = _templates.BuildOrderStatusHtml(
                brandName, nombreCliente, estadoDescripcion, numeroPedido, idEstado,
                supportEmail, brandCode, intentoEntrega, intentosMax, etiquetaLogistica);

            // Construir asunto con la etiqueta logística si está disponible
            var asunto = string.IsNullOrWhiteSpace(etiquetaLogistica)
                ? $"{estadoDescripcion} · Pedido #{numeroPedido:D6}"
                : $"Confirmación de Pedido #{numeroPedido:D6} [Relación: {etiquetaLogistica}]";

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