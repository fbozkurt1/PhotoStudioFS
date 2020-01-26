using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Models;

namespace PhotoStudioFS.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class PhotoController : BaseController
    {
        private readonly IOptions<AWSModel> appSettings;

        public PhotoController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context, IOptions<AWSModel> appSettings)
            : base(context, userManager, signInManager)
        {
            this.appSettings = appSettings;
        }

        public async Task<IActionResult> Index(int appointmentId, string customerId)
        {

            var customer = await userManager.FindByIdAsync(customerId);
            if (customer == null)
                return NotFound("Kullanıcı bulunamadı");
            var appointment = await unitOfWork.Appointments.Get(appointmentId);
            if (appointment == null)
                return NotFound("Randevu Bulunamadı");

            ViewBag.Customer = customer;
            ViewBag.Appointment = appointment;


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto(PhotoView photoView)
        {
            if (photoView.AppointmentId < 1
                || photoView.CustomerId.Equals("")
                || string.IsNullOrEmpty(photoView.CustomerId)
                || photoView.File == null)
            {
                return BadRequest("Bir sorun var");
            }

            if (!photoView.File.ContentType.Contains("image") || !photoView.File.ContentType.Contains("video"))
            {
                return BadRequest("Geçersiz Dosya Türü");
            }


            var customer = await userManager.FindByIdAsync(photoView.CustomerId);
            if (customer == null)
                return NotFound("Kullanıcı bulunamadı");
            var appointment = await unitOfWork.Appointments.Get(photoView.AppointmentId);
            if (appointment == null)
                return NotFound("Randevu Bulunamadı");



            var amazon = new AmazonS3Service(
                appSettings.Value.AccessKey,
                appSettings.Value.SecretAccessKey,
                appSettings.Value.BucketName);


            var response = await amazon.UploadFileAsync(photoView.File, photoView.CustomerId);

            if (response.Success)
            {
                var photo = new Photo()
                {
                    AppointmentId = appointment.Id,
                    CustomerId = customer.Id,
                    Path = response.PhotoUrl,
                    ThumbnailPath = response.ThumbnailUrl
                };
                try
                {
                    await unitOfWork.Photos.Add(photo);
                    await unitOfWork.Complete();

                    return Ok("Yükleme işlemi başarılı");
                }
                catch (Exception ex)
                {
                    return BadRequest("Yüklenemedi Hata oluştu: " + ex.Message);
                }
            }
            return BadRequest("Amazona yüklenemedi. Hata oluştu.");
            //foreach (IFormFile source in files)
            //{
            //    string filename = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.Trim('"');


            //    using (FileStream output = System.IO.File.Create(this.GetPathAndFilename(filename)))
            //        await source.CopyToAsync(output);
            //}


        }

        //private string GetPathAndFilename(string filename)
        //{
        //    return this.hostingEnvironment.WebRootPath + "\\uploads\\" + filename;
        //}
    }
}