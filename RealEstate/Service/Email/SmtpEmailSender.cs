using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace RealEstate.Service.Email
{
    public class SmtpEmailSender : IEmailAdvancedSender
    {
        private readonly IConfiguration _configuration;
        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var senderName = _configuration["EmailSettings:SenderName"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);
            await client.SendMailAsync(mailMessage);
        }
    }
}
