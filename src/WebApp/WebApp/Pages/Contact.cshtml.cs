using System;
using System.Web;
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
            mailToSend.Subject = $"Buchungsanfrage Görlitzer Ferienhaus von {inModel.Mail}";

            mailToSend.ContentText = "Die folgende Nachricht wurde über das Kontaktformular auf www.goerlitzer-ferienhaus.de abgesendet.\r\n";
            mailToSend.ContentText += "Der Datenschutzvereinbarung wurde zugestimmt.\r\n";
            mailToSend.ContentText += "----------------------------------------------------------------------------\r\n";
            mailToSend.ContentText += inModel.Content;

            mailToSend.ContentHtml = "Die folgende Nachricht wurde über das Kontaktformular auf www.goerlitzer-ferienhaus.de abgesendet.<br>";
            mailToSend.ContentHtml += "Der Datenschutzvereinbarung wurde zugestimmt.<br>";
            mailToSend.ContentHtml += "<hr>";
            mailToSend.ContentHtml += HttpUtility.HtmlEncode(inModel.Content);

            mailToSend.To = _appOptions.Value.ContactForm.To;
            mailToSend.ReplyTo = inModel.Mail;

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
