using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using WebApp;

namespace WebApp.Infrastructure
{
    public interface IMailSender
    {
        void Send(MailToSend mailToSend);
    }

    public class MailSender : IMailSender
    {
        private readonly IOptions<AppOptions> _appOptions;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailSender(IOptions<AppOptions> appOptions, IWebHostEnvironment webHostEnvironment)
        {
            _appOptions = appOptions;
            _webHostEnvironment = webHostEnvironment;
        }

        public void Send(MailToSend mailToSend)
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
                client.Connect(_appOptions.Value.Smpt.Server);
                client.Send(msg);
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
