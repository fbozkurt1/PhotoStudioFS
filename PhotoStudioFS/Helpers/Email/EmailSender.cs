using PhotoStudioFS.Models;
using PhotoStudioFS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace PhotoStudioFS.Helpers.Email
{
    public class EmailSender
    {

        private IRazorViewToStringRenderer _renderer;
        public EmailSender(IRazorViewToStringRenderer razorView)
        {
            _renderer = razorView;
        }
        public async Task<bool> SendNotifyEmail(string url, string templateName, string subject, MailReceiverInfo receiverInfo)
        {
            try
            {
                var from = new MailAddress("fuatbozkurt1@gmail.com", "Fuat Bozkurt");
                var to = new MailAddress(receiverInfo.Email);

                var model = new EmailButtonLinkViewModel(url, receiverInfo);

                string view = "/Views/Email/" + templateName;
                var htmlBody = await _renderer.RenderViewToStringAsync($"{view}Html.cshtml", model);
                //var textBody = await _renderer.RenderViewToStringAsync($"{view}Text.cshtml", model);

                var message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = ""
                };

                message.AlternateViews.Add(
                  AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html));

                using (var smtp = new SmtpClient("smtp.mailtrap.io", 587))
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential("7524a599e6e3e7", "a766ac65911fca");
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

    }
}
