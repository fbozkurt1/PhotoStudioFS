using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.Setting;

namespace PhotoStudioFS.Controllers
{
    public class SettingController : BaseController
    {
        private readonly IOptions<AWSModel> appSettings;

        public SettingController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context, IOptions<AWSModel> appSettings)
            : base(context, userManager, signInManager)
        {
            this.appSettings = appSettings;
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
        public async Task<IActionResult> CreateShootType(ShootTypeView shootTypeView)
        {
            if (ModelState.IsValid)
            {
                if (shootTypeView.Photo == null || !shootTypeView.Photo.ContentType.Contains("image"))
                {
                    return BadRequest("Geçersiz Dosya Türü Lütfen bir fotoğraf seçiniz.");
                }

                try
                {
                    var amazon = new AmazonS3Service(
                           appSettings.Value.AccessKey,
                           appSettings.Value.SecretAccessKey,
                           appSettings.Value.BucketName);


                    var response = await amazon.UploadFileAsync(file: shootTypeView.Photo, subFolderName: "ShootTypePhotos");

                    if (response.Success)
                    {
                        var shootType = new ShootType()
                        {
                            Name = shootTypeView.Name,
                            IsActive = shootTypeView.IsActive,
                            Icon = shootTypeView.Icon,
                            Description = shootTypeView.Description,
                            PhotoPath = response.ThumbnailUrl
                        };
                        await unitOfWork.ShootTypes.Add(shootType);
                        await unitOfWork.Complete();

                        return Ok("Çekim Türü başarıyla eklendi.");
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("NotShootType", "Bir Sorun oluştu, tekrar deneyiniz!");
                }
            }
            return View("ShootTypes", shootTypeView);

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