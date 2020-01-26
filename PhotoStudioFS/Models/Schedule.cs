using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PhotoStudioFS.Models
{
    public class Schedule
    {
        [Key]
        public int id { get; set; }

        [MaxLength(50)]
        public string title { get; set; }

        [DataType(DataType.Date), Required]
        public DateTime start { get; set; }

        [DataType(DataType.Date), Required]
        public DateTime end { get; set; }

        [MaxLength(20)]
        public string photoShootType { get; set; }

        public bool isEmpty { get; set; }

        public bool allDay { get; set; }
    }

    public class ScheduleView
    {
        [Key]
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

    }
}
