using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PhotoStudioFS.Data;
using Microsoft.AspNetCore.Identity;
using PhotoStudioFS.Models;
using System.Security.Claims;
using PhotoStudioFS.Helpers;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;

namespace PhotoStudioFS.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerSectionController : BaseController
    {
        private readonly IOptions<AWSModel> appSettings;

        public CustomerSectionController(photostudioContext _context, UserManager<User> _userManager,
            SignInManager<User> _signInManager, IOptions<AWSModel> appSettings)
            : base(_context, _userManager, _signInManager)
        {
            this.appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUser = await userManager.FindByIdAsync(currentUserId);

            var appointments = await unitOfWork.Appointments.GetAppointmentsByCustomer(currentUser.Id);
            ViewData["PageName"] = "Randevularım";
            return View(appointments);
        }


        [HttpGet]
        public async Task<IActionResult> GetPhotos(int appointmentId, string customerId)
        {
            if (appointmentId < 1 || string.IsNullOrEmpty(customerId) || string.IsNullOrWhiteSpace(customerId) || customerId == null)
            {
                return BadRequest("Randevu veya kullanıcı bilgisi alınamadı. Lütfen tekrar deneyiniz.");
            }
            if (await unitOfWork.Appointments.Get(appointmentId) == null)
            {
                return NotFound("Randevu bilgisi bulunamadı.");
            }
            var user = await userManager.FindByIdAsync(customerId);
            if (user == null)
            {
                return NotFound("Kullanıcı bilgisi bulunamadı.");
            }

            var photos = await unitOfWork.Photos.GetPhotos(appointmentId, customerId);
            ViewData["CustomerId"] = customerId;
            ViewData["AppointmentId"] = appointmentId;
            ViewData["UserFullName"] = user.FullName;
            ViewData["PageName"] = "Fotoğraflarım";
            return View("Photos", photos);

        }

        [HttpGet]
        public async Task<IEnumerable<Photo>> GetPhotosTemp(int appointmentId, string customerId)
        {
            var photos = await unitOfWork.Photos.GetPhotos(appointmentId, customerId);
            foreach (var photo in photos)
            {
                photo.FileName = Path.GetFileName(photo.Path);
            }
            return photos;
        }

        [HttpPost]
        public async Task<IActionResult> Download(PhotoViewToDownload photoView)
        {

            if (await userManager.FindByIdAsync(photoView.CustomerId) == null)
            {
                return BadRequest("Kullanıcı bulunamadı!");
            }
            if (await unitOfWork.Appointments.Get(photoView.AppointmentId) == null)
            {
                return BadRequest("Randevu Bulunamadı!");
            }

            var photos = await GetPhotosTemp(photoView.AppointmentId, photoView.CustomerId);

            if (!photos.Any())
            {
                return UnprocessableEntity("Kullanıcıya ait hiç fotoğraf bulunamadı");
            }

            var amazon = new AmazonS3Service(
               appSettings.Value.AccessKey,
               appSettings.Value.SecretAccessKey,
               appSettings.Value.BucketName);

            // the output bytes of the zip
            byte[] fileBytes = null;

            // create a working memory stream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // create a zip
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // interate through the source files
                    foreach (var photo in photos)
                    {
                        var list = photo.Path.Split(".com/");
                        string keyName = list[1];
                        var streamFile = await amazon.Download(keyName);
                        if (streamFile == null)
                        {
                            return UnprocessableEntity("Dosyalar indirelemedi! Lütfen tekrar deneyiniz.");
                        }
                        // add the item name to the zip
                        ZipArchiveEntry zipItem = zip.CreateEntry(photo.FileName);
                        // add the item bytes to the zip entry by opening the original file and copying the bytes

                        using (System.IO.Stream entryStream = zipItem.Open())
                        {
                            streamFile.CopyTo(entryStream);
                        }

                    }
                }
                fileBytes = memoryStream.ToArray();
            }

            // download the constructed zip
            Response.Headers.Add("Content-Disposition", "attachment; filename=download.zip");
            return File(fileBytes, "application/zip");
        }


        [HttpGet]
        public async Task<IActionResult> CreateAppointment()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUser = await userManager.FindByIdAsync(currentUserId);
            ViewBag.User = currentUser;
            return View();
        }

    }
}