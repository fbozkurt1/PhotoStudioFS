using Microsoft.AspNetCore.Http;
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

        [MaxLength(2000), Display(Name = "Açıklama"), Required]
        public string Description { get; set; }

        [MaxLength(1000), Required]
        public string PhotoPath { get; set; }
        [MaxLength(64), Required]
        public string Icon { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

    }

    public class ShootTypeView
    {

        [MaxLength(50), Display(Name = "Çekim Türü"), Required]
        public string Name { get; set; }

        [MaxLength(2000), Display(Name = "Açıklama"), Required]
        public string Description { get; set; }

        public IFormFile Photo { get; set; }

        public string Icon { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

    }
}
