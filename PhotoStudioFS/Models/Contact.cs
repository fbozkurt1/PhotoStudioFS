using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PhotoStudioFS.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50), Display(Name = "Adınız ve Soyadınız"), Required]
        public string Name { get; set; }

        [MaxLength(50), Display(Name = "Email Adresiniz"), Required, EmailAddress]
        public string Email { get; set; }

        [MaxLength(15), Display(Name = "Telefon Numaranız"), Required, Phone]
        public string Phone { get; set; }

        [MaxLength(200), Display(Name = "Hangi konu hakkında ?"), Required]
        public string Subject { get; set; }

        [MaxLength(2000), Display(Name = "İletmek istediğiniz mesaj..."), Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
