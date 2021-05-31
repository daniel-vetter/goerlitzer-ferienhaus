using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace WebApp.Infrastructure
{
    public interface IMailSender
    {
        Task Send(MailToSend mailToSend);
    }

    public class MailSender : IMailSender
    {
        private readonly IOptions<AppOptions> _appOptions;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MailSender> _logger;

        public MailSender(IOptions<AppOptions> appOptions, IWebHostEnvironment webHostEnvironment, ILogger<MailSender> logger)
        {
            _appOptions = appOptions;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task Send(MailToSend mailToSend)
        {
            try
            {
                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(mailToSend.From, mailToSend.From));
                foreach (var recipient in mailToSend.To)
                    msg.To.Add(new MailboxAddress(recipient, recipient));
                msg.Subject = mailToSend.Subject;
                if (!_webHostEnvironment.IsProduction())
                    msg.Subject += $" (Enviroment: {_webHostEnvironment.EnvironmentName})";
                msg.Body = new TextPart("plain", mailToSend.Content);
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_appOptions.Value.Smtp.Host, _appOptions.Value.Smtp.Port, _appOptions.Value.Smtp.Ssl);
                    await client.AuthenticateAsync(_appOptions.Value.Smtp.Username, _appOptions.Value.Smtp.Password);
                    await client.SendAsync(msg);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send mail from contact form.");
                throw;
            }
        }
    }

    public class MailToSend
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string From { get; set; }
        public string[] To { get; set; }
    }
}
