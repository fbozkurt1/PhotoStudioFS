using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Models.Setting
{
    public class ShootType
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50), Display(Name = "Çekim Türü"), Required]

        public string Name { get; set; }

        [Display(Name = "Çekim Türü"), Required]
        public bool IsActive { get; set; } = true;

    }
}
