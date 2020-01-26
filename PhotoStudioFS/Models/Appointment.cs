using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50), Display(Name = "Adı ve Soyadı"), Required]
        public string Name { get; set; }

        [MaxLength(50), Display(Name = "Email Adresi"), Required, EmailAddress]
        public string Email { get; set; }

        [MaxLength(15), Display(Name = "Telefon"), Required, Phone]
        public string Phone { get; set; }

        [MaxLength(20), Display(Name = "Çekim Türü"), Required]
        public string Type { get; set; }

        [MaxLength(1000), Display(Name = "Müşteri Mesajı")]
        public string Message { get; set; }

        [DataType(DataType.DateTime), Display(Name = "Randevu Tarihi Başlangıç"), Required]
        public DateTime AppointmentDateStart { get; set; }

        [DataType(DataType.DateTime), Display(Name = "Randevu Tarihi Bitiş"), Required]
        public DateTime AppointmentDateEnd { get; set; }

        [DataType(DataType.DateTime), Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Randevu Durumu"), Range(0, 2), Required]
        public short IsApproved { get; set; } = 0; // 0- Onay Bekliyor, 1- Onaylandı, 2- Ret

        public int ScheduleId { get; set; }

        [Display(Name = "Çekim Durumu"), Range(0, 2)]
        public short State { get; set; } = 0; // 0- bekliyor, 1- çekim tamamlandı (resimler hazırlanıyor), 2- Hazır (resimler yüklendi)

        [DataType(DataType.DateTime), Required]
        public DateTime StateUpdateDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(9, 2)"), Display(Name = "Çekim Ücreti"), Required]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(9, 2)"), Display(Name = "Ödenen Toplam Ücret")]
        public decimal PricePaid { get; set; } = 0;

        public string CustomerId { get; set; }
        public User Customer { get; set; }
    }
    public class AppointmentView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
        public string DateHourStart { get; set; }
        public string DateHourEnd { get; set; }
        public string CreatedAt { get; set; } = DateTime.Now.ToString();
        public short IsApproved { get; set; } = 0;
        public int ScheduleId { get; set; }
        public short State { get; set; } = 0; // 0- bekliyor, 1- çekim tamamlandı (resimler hazırlanıyor), 2- Hazır (resimler yüklendi)

    }

    public class AppointmentScheduleView
    {
        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string startHour { get; set; }
        public string end { get; set; }
        public string endHour { get; set; }
        public string photoShootType { get; set; }
        public bool isEmpty { get; set; }
        public bool allDay { get; set; }
        public string color { get; set; }
        public int scheduleId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string startDate { get; set; }
    }
}
