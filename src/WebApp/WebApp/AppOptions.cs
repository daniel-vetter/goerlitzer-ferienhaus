using MailKit.Security;

namespace WebApp
{
    public class AppOptions
    {
        public SmtpOptions Smtp { get; set; }
        public ContactFormOption ContactForm { get; set; }
    }

    public class ContactFormOption
    {
        public string From { get; set; }
        public string[] To { get; set; }
    }

    public class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public SecureSocketOptions Ssl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}