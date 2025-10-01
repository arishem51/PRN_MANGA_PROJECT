using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace PRN_MANGA_PROJECT.Services.Auth
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("thangmoneo2542004@gmail.com", "wywo rffe ubho gldr"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage("thangmoneo2542004@gmail.com", email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}
