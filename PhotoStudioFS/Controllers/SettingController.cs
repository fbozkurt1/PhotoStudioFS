using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoStudioFS.Data;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.Setting;

namespace PhotoStudioFS.Controllers
{
    public class SettingController : BaseController
    {
        public SettingController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context)
            : base(context, userManager, signInManager)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ShootTypes()
        {
            try
            {
                var shootTypes = await unitOfWork.ShootTypes.GetAll();
                ViewBag.ShootTypes = shootTypes;
            }
            catch (Exception e)
            {
                string ex = e.Message;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateShootType(ShootType shootType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await unitOfWork.ShootTypes.Add(shootType);
                    await unitOfWork.Complete();

                    return RedirectToAction("ShootTypes", "Setting");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("NotShootType", "Bir Sorun oluştu, tekrar deneyiniz!");
                }
            }
            return View("ShootTypes", shootType);

        }

        [HttpPost]
        public async Task<IActionResult> EditShootType(int id, ShootType shootType)
        {
            if (ModelState.IsValid)
            {
                if (await unitOfWork.ShootTypes.Get(id) == null)
                {
                    return UnprocessableEntity("Değiştirmek istediğiniz çekim türü bulunamadı. Lütfen tekrar deneyiniz!");
                }

                unitOfWork.ShootTypes.Update(shootType);
                await unitOfWork.Complete();
                return Ok("Başarıyla güncellendi!");
            }
            return BadRequest("Başarısız. Lütfen tekrar deneyiniz!");
        }
    }
}