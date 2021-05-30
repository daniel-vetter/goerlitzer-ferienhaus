namespace WebApp
{
    public class AppOptions
    {
        public SmptOptions Smpt { get; set; }
        public ContactFormOption ContactForm { get; set; }
    }

    public class ContactFormOption
    {
        public string From { get; set; }
        public string[] To { get; set; }
    }

    public class SmptOptions
    {
        public string Server { get; set; }
    }
}