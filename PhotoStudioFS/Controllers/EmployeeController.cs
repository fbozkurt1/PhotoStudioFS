using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.ViewModels;

namespace PhotoStudioFS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : BaseController
    {
        private IRazorViewToStringRenderer _renderer;
        private EmailSender emailSender;
        public EmployeeController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            photostudioContext context,
            IRazorViewToStringRenderer razorView)
             : base(context, userManager, signInManager)
        {
            _renderer = razorView;
            emailSender = new EmailSender(razorView);
        }

        public async Task<IActionResult> Index()
        {
            var users = await userManager.GetUsersInRoleAsync(Roles.Employee);
            users.Where(u => u.IsEnabled == true);
            return View(users);
        }

        public async Task<IActionResult> Details(string email)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
            {
                return NotFound();
            }
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {

            if (ModelState.IsValid)
            {
                if (await userManager.FindByEmailAsync(user.Email) != null)
                {
                    ModelState.AddModelError("NotUser", "Bu mail adresine kayıtlı personel zaten var. Bir mail adresine sahip yalnızca bir kullanıcı olabilir.");
                    return View(user);
                }
                user.UserName = user.Email;
                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Employee);

                    var user_ = await userManager.FindByEmailAsync(user.Email);
                    var token = userManager.GeneratePasswordResetTokenAsync(user_).Result;

                    var url = Url.Action("CreatePassword", "Account",
                            new { Token = token, Email = user_.Email, FullName = user_.FullName }, protocol: HttpContext.Request.Scheme);

                    var resultMail = await emailSender
                        .SendNotifyEmail(
                        url,
                        TemplateNames.CreatePassword,
                        "Hesabınız İçin Şifre Belirlemeniz Gerekiyor",
                        new MailReceiverInfo()
                        {
                            FullName = user_.FullName,
                            Date = user_.CreatedAt,
                            Type = "Employee",
                            Email = user_.Email
                        });


                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("NotUser", "Kullanıcı oluşturulamadı. Lütfen kurallara uygun olarak tekrar deneyiniz!");
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(string email)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
            {
                return NotFound();
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string Id, User user)
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrWhiteSpace(Id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var _user = await userManager.FindByIdAsync(Id);
                    if (_user == null)
                    {
                        return NotFound();
                    }
                    _user.Email = user.Email;
                    _user.FullName = user.FullName;
                    _user.PhoneNumber = user.PhoneNumber;
                    _user.IsEnabled = user.IsEnabled;
                    _user.CreatedAt = user.CreatedAt;
                    _user.UserName = user.Email;

                    var result = await userManager.UpdateAsync(_user);
                    if (!result.Succeeded)
                        return BadRequest("Kullanıcı güncellenemedi!");

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExistsAsync(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı");

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok("Başarıyla silindi!");

            return BadRequest("Kullanıcı silinemedi!");
        }

        private async Task<bool> UserExistsAsync(string id)
        {
            if (await userManager.FindByIdAsync(id) != null)
                return true;
            return false;
        }
    }
}
