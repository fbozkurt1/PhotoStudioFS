using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Models.ViewModels
{
    public class EmailButtonLinkViewModel
    {
        public string ButtonLink { get; set; }
        public MailReceiverInfo Info { get; set; }
        public EmailButtonLinkViewModel(string buttonLink, MailReceiverInfo receiverInfo)
        {
            ButtonLink = buttonLink;
            Info = receiverInfo;
        }
    }

    public class MailReceiverInfo
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
    }
}
