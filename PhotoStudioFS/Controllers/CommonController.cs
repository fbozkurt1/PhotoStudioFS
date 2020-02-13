using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.ViewModels;

namespace PhotoStudioFS.Controllers
{
    [AllowAnonymous]
    public class CommonController : BaseController
    {

        private IRazorViewToStringRenderer _renderer;
        private EmailSender emailSender;

        public CommonController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            photostudioContext context,
            IRazorViewToStringRenderer razorView)
            : base(context, userManager, signInManager)
        {
            _renderer = razorView;
            emailSender = new EmailSender(razorView);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllSchedules(string start, string end, int photoType)
        {

            List<ScheduleView> schedulesView = new List<ScheduleView>();
            DateTime dtStart = Convert.ToDateTime(start, CultureInfo.GetCultureInfo("tr-TR"));
            DateTime dtEnd;
            if (!string.IsNullOrEmpty(end))
            {
                dtEnd = Convert.ToDateTime(end, CultureInfo.GetCultureInfo("tr-TR"));
            }
            else
            {
                dtEnd = dtStart.AddDays(1);
            }
            IEnumerable<Schedule> schedules;

            if (photoType == 0)
            {
                schedules = await unitOfWork.Schedules.GetSchedules(dtStart, dtEnd);
            }
            else
            {
                var shootType = await unitOfWork.ShootTypes.Get(photoType);
                if (shootType == null)
                {
                    return null;
                }
                schedules = await unitOfWork.Schedules.GetSchedulesByPhotoType(dtStart, dtEnd, photoType);
            }
            if (schedules != null && schedules.Any())
            {
                foreach (var schedule in schedules)
                {
                    schedulesView.Add(new ScheduleView()
                    {
                        id = schedule.id,
                        allDay = schedule.allDay,
                        isEmpty = schedule.isEmpty,
                        start = schedule.start.ToString("yyyy-MM-ddTHH:mm"),
                        startHour = schedule.start.ToString("HH:mm").Trim(),
                        end = schedule.end.ToString("yyyy-MM-ddTHH:mm"),
                        endHour = schedule.end.ToString("HH:mm").Trim(),
                        title = schedule.ShootType.Name,
                        photoShootType = schedule.ShootType.Name,
                        photoShootTypeId = schedule.ShootType.Id,
                        color = schedule.isEmpty == true ? "#6ced15" : "#ed4734"
                    });
                }
            }
            return Json(schedulesView);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppointmentView appointmentView)
        {

            if (ModelState.IsValid)
            {
                var schedule = await unitOfWork.Schedules.Find(s => s.id == appointmentView.ScheduleId && s.isEmpty == true);
                if (!schedule.Any())
                {
                    return BadRequest("Randevu almak istediğiniz tarih dolu ve ScheduleId göndermediniz!");
                }

                var appointment = new Appointment()
                {
                    Name = appointmentView.Name,
                    Phone = appointmentView.Phone,
                    Email = appointmentView.Email,
                    Message = appointmentView.Message,
                    AppointmentDateStart = schedule.First().start,
                    AppointmentDateEnd = schedule.First().end,
                    CreatedAt = DateTime.Now,
                    IsApproved = 0,
                    ShootTypeId = appointmentView.ShootTypeId,
                    ScheduleId = appointmentView.ScheduleId
                };

                if (!await UpdateScheduleIsEmptyField(appointmentView.ScheduleId, false))
                    return NotFound("Schedule Bulunamadı");

                await unitOfWork.Appointments.Add(appointment);
                await unitOfWork.Complete();

                var url = Url.Action("Index", "Home",
                            new { }, protocol: HttpContext.Request.Scheme);

                var resultMail = await emailSender
                    .SendNotifyEmail(url,
                        TemplateNames.AppointmentRequest,
                        "Randevunuz Talebiniz Oluşturuldu",
                        new MailReceiverInfo()
                        {
                            FullName = appointment.Name,
                            Email = appointment.Email,
                            Date = appointment.AppointmentDateStart
                        });

                return Ok("Randevu talebiniz başarıyla eklendi. Talebiniz onaylandığında mail ile bilgilendirileceksiniz.");
            }
            else
            {
                return BadRequest("hata");
            }

        }
        private async Task<bool> UpdateScheduleIsEmptyField(int id, bool isEmpty)
        {
            try
            {
                var schedule = await unitOfWork.Schedules.Get(id);
                if (schedule == null)
                    return false;
                schedule.isEmpty = isEmpty;
                unitOfWork.Schedules.Update(schedule);
                await unitOfWork.Complete();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}