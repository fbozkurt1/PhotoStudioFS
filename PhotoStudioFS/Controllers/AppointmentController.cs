using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Helpers.Extensions.Alerts;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.ViewModels;

namespace PhotoStudioFS.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
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


        [HttpGet]
        public async Task<IActionResult> Create(string email, string redirectTo, string error)
        {
            var customer = await userManager.FindByEmailAsync(email);
            if (customer == null)
            {
                return Redirect(redirectTo);

            }
            var appointment = new Appointment()
            {
                Customer = customer,
                CustomerId = customer.Id,
                Name = customer.FullName,
                Email = customer.Email,
                Phone = customer.PhoneNumber,
                IsApproved = 1,
                State = 0
            };
            ViewBag.ShootTypes = await unitOfWork.ShootTypes.Find(s => s.IsActive == true);
            ViewBag.RedirectTo = Request.Headers["Referer"];
            if (error is null)
                return View(appointment);
            else
                return View(appointment).WithDanger("Hata!", error);
        }

        [HttpPost]
        public async Task<IActionResult> CreateToCustomer(Appointment appointment, string redirectTo)
        {

            if (ModelState.IsValid)
            {
                var customer = await userManager.FindByIdAsync(appointment.CustomerId);
                if (customer == null)
                {
                    ModelState.AddModelError("NotUser", "Lütfen müşteriyi seçip tekrar deneyiniz!");
                    return RedirectToAction("Create",
                        new
                        {
                            email = appointment.Email,
                            redirectTo = redirectTo,
                            error = "Müşterinin verileri bulunamadı! Lütfen müşteriyi seçip tekrar deneyiniz."
                        });
                }
                var shootType = await unitOfWork.ShootTypes.Get(appointment.ShootTypeId);
                if (shootType == null || !shootType.IsActive)
                {
                    ModelState.AddModelError("NotShootType", "Lütfen Çekim Türü seçiniz!");
                    return View();
                }
                Schedule schedule = new Schedule()
                {
                    allDay = false,
                    start = appointment.AppointmentDateStart,
                    end = appointment.AppointmentDateEnd,
                    isEmpty = false,
                    ShootTypeId = appointment.ShootTypeId,
                };

                await unitOfWork.Schedules.Add(schedule);
                await unitOfWork.Complete();

                appointment.ScheduleId = schedule.id;

                await unitOfWork.Appointments.Add(appointment);
                await unitOfWork.Complete();

                await SendNotify(appointment, new Appointment() { State = 0, IsApproved = 0, Email = appointment.Email }, customer);

                if (redirectTo != null && !redirectTo.Equals("") && !string.IsNullOrEmpty(redirectTo))
                {
                    return Redirect(redirectTo);
                }
                return RedirectToAction("Details", "Customer", new { email = appointment.Email });
            }
            return RedirectToAction("Create", "Appointment", new { email = appointment.Email, redirectTo = redirectTo });
        }

        public async Task<IActionResult> Edit(int id)
        {

            var appointment = await unitOfWork.Appointments.GetAppointment(id);

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
                    var shootType = await unitOfWork.ShootTypes.Get(appointment.ShootTypeId);
                    if (shootType == null)
                    {
                        ModelState.AddModelError("NotUser", "Lütfen Çekim Türü seçiniz");
                        return View(appointment);
                    }
                    var oldAppointment = await unitOfWork.Appointments.Get(appointment.Id);
                    if (oldAppointment == null)
                    {
                        ModelState.AddModelError("NotUser", "Lütfen tüm alanları doldurunuz!");
                        return View(appointment);
                    }

                    bool scheduleIsEmpty = appointment.IsApproved == 2 ? true : false;

                    if (!await UpdateScheduleIsEmptyField(oldAppointment.ScheduleId, scheduleIsEmpty))
                    {
                        ModelState.AddModelError("NotUser", "Lütfen tüm alanları doldurunuz!");
                        return View(appointment);
                    }
                    if (oldAppointment.Customer == null)
                    {
                        var customer = await AddUserIfNotExist(oldAppointment);
                        if (customer == null)
                        {
                            ModelState.AddModelError("NotUser", "Müşteri hesabı oluşturulamadı. Lütfen tekrar deneyiniz!");
                            return View(appointment);
                        }
                        oldAppointment.CustomerId = customer.Id;
                        oldAppointment.Customer = customer;
                    }
                    if (!await SendNotify(appointment, oldAppointment, oldAppointment.Customer))
                    {
                        ModelState.AddModelError("NotUser2", "Kullanıcıya randevusuyla ilgili bildirim gönderilemedi. Lütfen tekrar deneyiniz!");
                        return View(appointment);
                    }

                    oldAppointment.IsApproved = appointment.IsApproved;
                    oldAppointment.State = appointment.State;
                    oldAppointment.Price = appointment.Price;
                    oldAppointment.PricePaid = appointment.PricePaid;
                    oldAppointment.AppointmentDateStart = appointment.AppointmentDateStart;
                    oldAppointment.AppointmentDateEnd = appointment.AppointmentDateEnd;
                    unitOfWork.Appointments.Update(oldAppointment);
                    await unitOfWork.Complete();
                }
                catch (Exception)
                {

                    ModelState.AddModelError("NotUser2", "Lütfen tüm alanları doldurunuz!");
                    return View(appointment);

                }
                return Redirect(redirectTo);

            }
            ModelState.AddModelError("NotUser", "Lütfen tüm alanları doldurunuz!");
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

        private async Task<User> AddUserIfNotExist(Appointment appointment)
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

                    var token = userManager.GeneratePasswordResetTokenAsync(user).Result;

                    var url = Url.Action("CreatePassword", "Account",
                        new { Token = token, Email = user.Email, FullName = user.FullName }, protocol: HttpContext.Request.Scheme);

                    var resultMail = await emailSender.SendNotifyEmail(
                        url,
                        TemplateNames.CreatePassword,
                        "Hesabınız Oluşturuldu!",
                        new MailReceiverInfo()
                        {
                            FullName = user.FullName,
                            Email = user.Email,
                            Type = "NewAccount",
                            Date = DateTime.Now
                        });

                    return user;
                }
                return null;
            }
            return user_;
        }

        private async Task<bool> SendNotify(Appointment appointment, Appointment oldAppointment, User user)
        {
            var url = Url.Action("Login", "Account", new { }, protocol: HttpContext.Request.Scheme);
            bool isAppointmentNotifySent = true, isPhotoStateNotifySent = true;
            // State of appointment
            if (appointment.IsApproved != oldAppointment.IsApproved)
            {
                string template = "", subject = "";
                bool isWillSend = false;
                if (appointment.IsApproved == 1) // if appointment is approved
                {
                    template = TemplateNames.AppointmentApproved;
                    subject = "Randevunuz Onaylandı";
                    isWillSend = true;
                }
                else if (appointment.IsApproved == 2) // if appointment is rejected
                {
                    template = TemplateNames.AppointmentRejected;
                    subject = "Randevunuz Maalesef Onaylanamadı :(";
                    isWillSend = true;
                }
                if (isWillSend)
                {
                    isAppointmentNotifySent = await emailSender.SendNotifyEmail(
                               url,
                               template,
                               subject,
                               new MailReceiverInfo()
                               {
                                   FullName = user.FullName,
                                   Email = user.Email,
                                   Date = oldAppointment.AppointmentDateStart,
                               });
                }
            }
            // State of photo shoot
            if (appointment.State != oldAppointment.State)
            {
                string template = "", subject = "", emailType = "";
                bool isWillSend = false;
                template = TemplateNames.PhotoStatesNotify;

                if (appointment.State == 1)
                {
                    subject = "Fotoğraflarınız/Videolarınız Hazırlanıyor";
                    emailType = "Preparing";
                    isWillSend = true;
                }
                else if (appointment.State == 2)
                {
                    subject = "Fotoğraflarınız/Videolarınız Hazır!";
                    emailType = "Ready";
                    isWillSend = true;
                }
                if (isWillSend)
                {
                    isPhotoStateNotifySent = await emailSender.SendNotifyEmail(
                           url,
                           template,
                           subject,
                           new MailReceiverInfo()
                           {
                               FullName = user.FullName,
                               Email = user.Email,
                               Date = oldAppointment.AppointmentDateStart,
                               Type = emailType
                           });
                }
            }

            return isAppointmentNotifySent && isPhotoStateNotifySent;

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