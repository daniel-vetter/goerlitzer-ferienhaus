using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using WebApp;
using WebApp.Infrastructure;

namespace WebPage.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IMailSender _mailSender;
        private readonly IOptions<AppOptions> _appOptions;

        public ContactModel(IMailSender mailSender, IOptions<AppOptions> appOptions)
        {
            _mailSender = mailSender;
            _appOptions = appOptions;
        }

        public void OnGet()
        {
        }

        public ActionResult OnPostSendRequest(SendRequestModel inModel)
        {
            if (inModel.Approval == false)
                throw new Exception("Approval not given.");
            
            var mailToSend = new MailToSend();
            mailToSend.From = _appOptions.Value.ContactForm.From;
            mailToSend.Subject = $"Eine Nachricht wurde über das Kontaktformular auf www.goerlitzer-ferienhaus.de verschickt. (Absender: {inModel.Mail})";
            mailToSend.Content = "Die folgende Nachricht wurde über das Kontaktformular auf www.goerlitzer-ferienhaus.de abgesendet.\r\n";
            mailToSend.Content += "Der Datenschutzvereinbarung wurde zugestimmt.\r\n";
            mailToSend.Content += "----------------------------------------------------------------------------\r\n";
            mailToSend.Content += inModel.Content;
            mailToSend.To = _appOptions.Value.ContactForm.To;
            _mailSender.Send(mailToSend);

            return new JsonResult(new
            {
                Success = true
            });
        }
    }

    public class SendRequestModel
    {
        public bool Approval { get; set; }
        public string Mail { get; set; }
        public string Content { get; set; }
    }
}
