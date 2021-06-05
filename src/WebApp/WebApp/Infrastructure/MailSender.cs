using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Infrastructure
{
    public interface IMailSender
    {
        Task Send(MailToSend mailToSend);
    }

    public class MailSender : IMailSender, IHealthCheck
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

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = await CreatSmptClient())
                {
                    await client.NoOpAsync();
                }
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy("MailSender could not connect to the smtp service.", e);
            }
            
            return HealthCheckResult.Healthy();
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
                if (mailToSend.ReplyTo != null)
                {
                    if (MailboxAddress.TryParse(mailToSend.ReplyTo, out var address))
                        msg.ReplyTo.Add(address);
                    else
                        _logger.LogWarning($"Could not parse mail address '{mailToSend.ReplyTo}'. It will not be set as ReplyTo address.");
                }
                var b = new BodyBuilder();
                b.HtmlBody = mailToSend.ContentHtml;
                b.TextBody = mailToSend.ContentText;
                msg.Body = b.ToMessageBody();

                using (var client = await CreatSmptClient())
                {
                    
                    await client.SendAsync(msg);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send mail from contact form.");
                throw;
            }
        }

        private async Task<SmtpClient> CreatSmptClient()
        {
            var client = new SmtpClient();
            await client.ConnectAsync(_appOptions.Value.Smtp.Host, _appOptions.Value.Smtp.Port, _appOptions.Value.Smtp.Ssl);
            if (!string.IsNullOrWhiteSpace(_appOptions.Value.Smtp.Username) || !string.IsNullOrWhiteSpace(_appOptions.Value.Smtp.Password))
                await client.AuthenticateAsync(_appOptions.Value.Smtp.Username, _appOptions.Value.Smtp.Password);
            return client;
        }
    }

    public class MailToSend
    {
        public string Subject { get; set; }
        public string ContentText { get; set; }
        public string ContentHtml { get; set; }
        public string From { get; set; }
        public string[] To { get; set; }
        public string ReplyTo { get; set; }
    }
}
