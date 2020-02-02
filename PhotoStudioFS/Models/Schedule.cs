using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using PhotoStudioFS.Models.Setting;

namespace PhotoStudioFS.Models
{
    public class Schedule
    {
        [Key]
        public int id { get; set; }

        [DataType(DataType.Date), Required]
        public DateTime start { get; set; }

        [DataType(DataType.Date), Required]
        public DateTime end { get; set; }

        public int ShootTypeId { get; set; }
        public ShootType ShootType { get; set; }

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
        public int photoShootTypeId { get; set; }
        public bool isEmpty { get; set; } = true;
        public bool allDay { get; set; } = false;
        public string color { get; set; }
    }

}
