using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Back.Services
{
    public class EmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailSender(IOptions<SmtpSettings> smtpOptions)
        {
            _smtpSettings = smtpOptions.Value;
        }

        public async Task EnviarCorreoCambioEstado(string destinatario, string nombreCliente, string estadoPedido, int numeroPedido)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_smtpSettings.User, "Farmacia"),
                Subject = $"Estado de tu pedido #{numeroPedido}",
                Body = $"Hola {nombreCliente},\n\nEl estado de tu pedido #{numeroPedido} cambió a: {estadoPedido}.\n\n¡Gracias por confiar en nosotros!",
                IsBodyHtml = false
            };
            mail.To.Add(destinatario);

            using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
            {
                client.EnableSsl = _smtpSettings.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password);

                await client.SendMailAsync(mail);
            }
        }
    }
}