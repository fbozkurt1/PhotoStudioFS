using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoStudioFS.Data;
using PhotoStudioFS.Models;

namespace PhotoStudioFS.Controllers
{
    
    public class ContactController : BaseController
    {
        public ContactController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context)
             : base(context, userManager, signInManager)
        {
        }
        public async Task<IActionResult> Index()
        {
            var unReadContactRequests = await unitOfWork.Contacts.GetContactRequests(false);
            ViewBag.ReadContactRequests = await unitOfWork.Contacts.GetContactRequests(true);
            return View(unReadContactRequests);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Contact contact)
        {
            if (ModelState.IsValid)
            {
                await unitOfWork.Contacts.Add(contact);
                int result = await unitOfWork.Complete();
                if (result > 0)
                {
                    return Ok("İletişim isteğiniz başarıyla gönderildi!");
                }
                return BadRequest("Bir sorun oluştu! Lütfen tekrar deneyiniz.");
            }

            return BadRequest("Lütfen tüm alanları doldurunuz!");

        }

        [HttpPost]
        public async Task<IActionResult> UpdateIsRead(int id)
        {
            var contact = await unitOfWork.Contacts.Get(id);
            if (contact == null)
            {
                return NotFound("İletişim isteği bulunamadı");
            }

            contact.IsRead = !contact.IsRead;

            unitOfWork.Contacts.Update(contact);
            if (await unitOfWork.Complete() > 0)
            {
                return Ok("Güncelleme başarılı");
            }
            return UnprocessableEntity("Güncelleme başarısız");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contact = await unitOfWork.Contacts.Get(id);
            if (contact == null)
            {
                return UnprocessableEntity("İletişim isteği bulunamadı!");
            }

            try
            {
                unitOfWork.Contacts.Remove(contact);
                await unitOfWork.Complete();
                return Ok("Başarıyla Silindi");
            }
            catch (Exception)
            {
                return BadRequest("İletişim isteği silinemedi!");
            }
        }
    }
}