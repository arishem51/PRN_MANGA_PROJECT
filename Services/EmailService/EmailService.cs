using System.Net.Mail;
using System.Net;

namespace PRN_MANGA_PROJECT.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["EmailSettings:FromEmail"];
            var fromPassword = _config["EmailSettings:AppPassword"];

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            var mail = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body
            };

            await smtp.SendMailAsync(mail);
        }
    }
    }
