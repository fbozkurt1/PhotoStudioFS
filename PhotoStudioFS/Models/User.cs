using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Models
{
    public class User : IdentityUser
    {
        [MaxLength(50), Display(Name = "Adı ve Soyadı"), Required]
        public string FullName { get; set; }

        [Display(Name = "Aktif mi ?")]
        public bool IsEnabled { get; set; } = true;

        public bool IsFirstLogin { get; set; } = true;

        [DataType(DataType.Date), Display(Name = "Kayıt Tarihi"), Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public IList<Photo> Photos { get; set; }
        public IList<Appointment> Appointments { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Lütfen e-posta adresini boş geçmeyiniz.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Lütfen uygun formatta e-posta adresi giriniz.")]
        [Display(Name = "E-Posta Adresiniz")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Lütfen şifreyi boş geçmeyiniz.")]
        [DataType(DataType.Password, ErrorMessage = "Lütfen uygun formatta şifre giriniz.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; }


        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }

    }

    public class ForgotPasswordViewModel
    {
        public string FullName { get; set; }

        [Required(ErrorMessage = "Lütfen e-posta adresini boş geçmeyiniz.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Lütfen uygun formatta e-posta adresi giriniz.")]
        [Display(Name = "E-Posta Adresiniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lütfen şifreyi boş geçmeyiniz.")]
        [DataType(DataType.Password, ErrorMessage = "Lütfen uygun formatta şifre giriniz.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Lütfen şifrenizi onaylayınız.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrenizi Onaylayınız")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
