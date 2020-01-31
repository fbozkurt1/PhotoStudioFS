using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.ViewModels;

namespace PhotoStudioFS.Controllers
{
    public class AppointmentController : BaseController
    {
        private IRazorViewToStringRenderer _renderer;
        private EmailSender emailSender;

        public AppointmentController(
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
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetAppointments(string start, string end, short isApproved)
        {
            List<AppointmentScheduleView> appointmentSchedulesView = new List<AppointmentScheduleView>();
            DateTime dtStart = Convert.ToDateTime(start, CultureInfo.GetCultureInfo("tr-TR"));
            DateTime dtEnd = Convert.ToDateTime(end, CultureInfo.GetCultureInfo("tr-TR"));

            var appointments = await unitOfWork.Appointments.GetAppointmentsByIsApproved(isApproved);
            foreach (var appointment in appointments)
            {

                appointmentSchedulesView.Add(new AppointmentScheduleView()
                {
                    id = appointment.Id,
                    allDay = false,
                    start = appointment.AppointmentDateStart.ToString("yyyy-MM-ddTHH:mm"),
                    startHour = appointment.AppointmentDateStart.ToString("HH:mm").Trim(),
                    end = appointment.AppointmentDateEnd.ToString("yyyy-MM-ddTHH:mm"),
                    endHour = appointment.AppointmentDateEnd.ToString("HH:mm").Trim(),
                    title = appointment.Name,
                    photoShootType = appointment.ShootType.Name,
                    photoShootTypeId = appointment.ShootType.Id,
                    color = appointment.AppointmentDateStart >= DateTime.Now ? "#6ced15" : "#ed4734",
                    scheduleId = appointment.ScheduleId,
                    name = appointment.Name,
                    email = appointment.Email,
                    phone = appointment.Phone,
                    startDate = appointment.AppointmentDateStart.ToString("dd/MM/yyyy")
                });
            }


            return Json(appointmentSchedulesView);

        }
        public async Task<IActionResult> GetRequests()
        {
            var appointments = await unitOfWork.Appointments.GetAppointmentsByIsApproved(0);
            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppointmentView appointmentView)
        {

            if (ModelState.IsValid)
            {
                var appointment = new Appointment()
                {
                    Name = appointmentView.Name,
                    Phone = appointmentView.Phone,
                    Email = appointmentView.Email,
                    Message = appointmentView.Message,
                    AppointmentDateStart = Convert.ToDateTime(appointmentView.Date + " " + appointmentView.DateHourStart, CultureInfo.GetCultureInfo("tr-TR")),
                    AppointmentDateEnd = Convert.ToDateTime(appointmentView.Date + " " + appointmentView.DateHourEnd, CultureInfo.GetCultureInfo("tr-TR")),
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
                        "Randevunuz Talebiniz İletildi",
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await unitOfWork.Appointments.Get((int)id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["RedirectTo"] = Request.Headers["Referer"];
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment, string redirectTo)
        {
            if (id != appointment.Id || string.IsNullOrEmpty(redirectTo) || string.IsNullOrWhiteSpace(redirectTo))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldAppointment = await unitOfWork.Appointments.Get(appointment.Id);
                    if (oldAppointment == null)
                    {
                        return NotFound();
                    }

                    bool scheduleIsEmpty = appointment.IsApproved == 2 ? true : false;

                    if (!await UpdateScheduleIsEmptyField(appointment.ScheduleId, scheduleIsEmpty))
                        return NotFound("Schedule Bulunamadı");

                    if (appointment.IsApproved == 1)
                    {
                        var customer = await AddUser(appointment, oldAppointment);
                        if (customer == null)
                        {
                            return NotFound("Kullanıcı Eklenemedi");
                        }
                        appointment.CustomerId = customer.Id;

                    }

                    unitOfWork.Appointments.Update(appointment);
                    await unitOfWork.Complete();
                }
                catch (DbUpdateConcurrencyException)
                {

                    return NotFound();

                }
                //return RedirectToAction(nameof(GetRequests));
                return Redirect(redirectTo);

            }
            return View(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string email)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email) || email.Equals(""))
            {
                return BadRequest("Randevu silinemedi. Çünkü email alanı boş!");
            }

            var appointment = await unitOfWork.Appointments.Get(id);
            if (appointment == null)
            {
                return BadRequest("Randevu silinemedi çünkü randevu bulunamadı");
            }

            try
            {
                unitOfWork.Appointments.Remove(appointment);
                await unitOfWork.Complete();

                return Ok("Başarıyla silindi");
            }
            catch (Exception)
            {
                return BadRequest("Randevu silinemedi çünkü randevu silinirken bir hata oluştu!");
            }


        }

        private async Task<User> AddUser(Appointment appointment, Appointment oldAppointment)
        {
            try
            {
                var user_ = await userManager.FindByEmailAsync(appointment.Email);
                if (user_ == null)
                {
                    var user = new User()
                    {
                        FullName = appointment.Name,
                        Email = appointment.Email,
                        UserName = appointment.Email,
                        PhoneNumber = appointment.Phone
                    };

                    var result = await userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, Roles.Customer);
                        user_ = await userManager.FindByEmailAsync(user.Email);
                        var token = userManager.GeneratePasswordResetTokenAsync(user_).Result;

                        var url = Url.Action("CreatePassword", "Account",
                            new { Token = token, Email = user_.Email, FullName = user_.FullName }, protocol: HttpContext.Request.Scheme);

                        var resultMail = await emailSender.SendNotifyEmail(
                            url,
                            TemplateNames.CreatePassword,
                            "Randevunuz Onaylandı",
                            new MailReceiverInfo()
                            {
                                FullName = appointment.Name,
                                Email = appointment.Email,
                                Type = "Appointment",
                                Date = appointment.AppointmentDateStart
                            });

                        return user;
                    }
                    else
                        return null;
                }
                else
                {
                    if (appointment.State != oldAppointment.State && appointment.State == 1)
                    {
                        var url = Url.Action("Login", "Account", new { }, protocol: HttpContext.Request.Scheme);

                        var resultMail = await emailSender.SendNotifyEmail(
                            url,
                            TemplateNames.AppointmentApproved,
                            "Randevunuz Onaylandı",
                            new MailReceiverInfo()
                            {
                                FullName = appointment.Name,
                                Email = appointment.Email,
                                Date = appointment.AppointmentDateStart
                            });
                    }
                    return user_;
                }

            }
            catch (Exception)
            {
                return null;
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
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}