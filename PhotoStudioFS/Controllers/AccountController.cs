using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.ViewModels;

namespace PhotoStudioFS.Controllers
{
    public class AccountController : BaseController
    {
        private IRazorViewToStringRenderer _renderer;
        private EmailSender emailSender;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context, IRazorViewToStringRenderer razorView)
        : base(context, userManager, signInManager)
        {
            _renderer = razorView;
            emailSender = new EmailSender(razorView);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(loginViewModel.Email);
                if (user == null)
                {
                    ModelState.AddModelError("NotUser", "Böyle bir kullanıcı bulunmamaktadır.");
                    ModelState.AddModelError("NotUser2", "E-posta veya şifre yanlış.");
                }
                else
                {
                    await signInManager.SignOutAsync();
                    var result = await signInManager.PasswordSignInAsync(
                        user, loginViewModel.Password, loginViewModel.RememberMe, false);
                    if (result.Succeeded)
                    {
                        var userRoleList = await userManager.GetRolesAsync(user);

                        if (userRoleList.IndexOf(Roles.Admin) >= 0 || userRoleList.IndexOf(Roles.Employee) >= 0)
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (userRoleList.IndexOf(Roles.Customer) >= 0)
                        {
                            return RedirectToAction("Index", "CustomerSection");
                        }

                    }
                    else
                    {
                        return NotFound("Bulunamadı");
                    }
                }

            }
            return View(loginViewModel);

        }

        [HttpGet]
        public async Task<IActionResult> CreatePassword(ForgotPasswordViewModel forgotPasswordView)
        {
            var user = await userManager.FindByEmailAsync(forgotPasswordView.Email);

            if (user == null || string.IsNullOrEmpty(forgotPasswordView.Token) || string.IsNullOrWhiteSpace(forgotPasswordView.Token))
            {
                return BadRequest("Şifrenizi oluşturmak veya sıfırlamak için gereken veriler geçerli değil." +
                    " Lütfen site yöneticisi ile iletişime geçiniz!");

            }
            if (!await userManager.VerifyUserTokenAsync(
                user,
                userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                forgotPasswordView.Token))
            {
                ModelState.AddModelError("NotUser",
                    "Şifrenizi oluşasdsadsadereken verileasdsadasdl." +
                    " Lütfen site yönetimi ile iletişime geçiniz!");

                return BadRequest("Size verilen linki zaten kullanmışsınız veya süresi dolmuş. " +
                    "Eğer yanlışlık olduğunu düşünüyorsanız, Lütfen site yönetimiyle iletişime geçiniz.");
            }

            return View(forgotPasswordView);

        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError("NotUser", "Böyle bir kullanıcı bulunmamaktadır.");
            }
            else
            {
                var token = userManager.GeneratePasswordResetTokenAsync(user).Result;
                var url = Url.Action("CreatePassword", "Account",
                               new { Token = token, Email = user.Email, FullName = user.FullName }, protocol: HttpContext.Request.Scheme);

                var resultMail = await emailSender.SendNotifyEmail(
                    url,
                    TemplateNames.CreatePassword,
                    "Şifre Sıfırlama Talebi",
                    new MailReceiverInfo()
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Type = "ForgotPassword",
                        Date = DateTime.Now
                    });
                ViewBag.Success = "İşlem başarılı";
            }

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ForgotPasswordViewModel forgotPasswordView)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(forgotPasswordView.Email);
                if (user == null || string.IsNullOrEmpty(forgotPasswordView.Token) || string.IsNullOrWhiteSpace(forgotPasswordView.Token))
                {
                    return NotFound($"{forgotPasswordView.Email} mail adresine kayıtlı bir kullanıcı bulunamadı.");
                }

                var result = userManager.ResetPasswordAsync(user, forgotPasswordView.Token, forgotPasswordView.Password).Result;

                if (result.Succeeded)
                {
                    return Ok("Şifreniz başarıyla değiştirildi.");
                }

            }

            return View("ForgotPassword", forgotPasswordView);
        }

        public async Task Logout()
        {
            await signInManager.SignOutAsync();
        }
    }
}