using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.Setting;
using PhotoStudioFS.Models.ViewModels;

namespace Api.PhotoStudioFS.Controllers
{
    [Route("deneme/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private IRazorViewToStringRenderer _renderer;
        private EmailSender emailSender;
        protected UnitOfWork unitOfWork { get; private set; }
        protected photostudioContext context { get; set; }
        protected UserManager<User> userManager { get; private set; }
        protected SignInManager<User> signInManager { get; private set; }
        private readonly IConfiguration configuration;
        public ApiController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            photostudioContext context,
            IRazorViewToStringRenderer razorView)
        {
            this.context = context;
            unitOfWork = new UnitOfWork(context);
            _renderer = razorView;
            emailSender = new EmailSender(razorView);
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        /* APPOINTMENT REQUESTS */

        [HttpGet("GetAvailableAppointmentDates")]
        public async Task<ActionResult<IEnumerable<ScheduleView>>> GetAvailableAppointmentDates(string start, int photoShootType)
        {
            if (string.IsNullOrEmpty(start) || string.IsNullOrWhiteSpace(start) || start == null)
                return BadRequest("start değeri boş olamaz!");

            //if (string.IsNullOrEmpty(photoShootType) || string.IsNullOrWhiteSpace(photoShootType) || photoShootType == null)
            //    return BadRequest("photoShootType değeri boş olamaz!");

            DateTime dtStart = Convert.ToDateTime(start, CultureInfo.GetCultureInfo("tr-TR"));
            DateTime dtEnd = dtStart.AddDays(1);

            try
            {
                var schedules = await unitOfWork.Schedules.GetSchedulesByPhotoType(dtStart, dtEnd, photoShootType);
                List<ScheduleView> schedulesView = new List<ScheduleView>();
                if (schedules != null)
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
                return Ok(schedulesView);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        [HttpPost("AddAppointmentRequest")]
        public async Task<ActionResult<Appointment>> AddAppointmentRequest([FromForm]AppointmentView appointmentView)
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

                try
                {
                    if (await unitOfWork.Schedules.Find(s => s.id == appointmentView.ScheduleId && s.isEmpty == true) == null)
                    {
                        return BadRequest("Randevu almak istediğiniz tarih dolu!");
                    }

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
                    return CreatedAtAction("GetAppointment", new { id = appointment.Id }, appointment);

                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Hata tekrar deneyiniz.");
                }

            }
            else
            {
                return UnprocessableEntity("Data uygun formatta değil.");
            }

        }

        [HttpGet("GetAppointments")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments(string customerId)
        {
            if (string.IsNullOrEmpty(customerId) || string.IsNullOrWhiteSpace(customerId) || customerId == null)
            {
                return UnprocessableEntity("customerId boş!");
            }
            try
            {
                var currentUser = await userManager.FindByIdAsync(customerId);
                if (currentUser == null)
                {
                    return NotFound("Kullanıcı bulunamadı");
                }
                var appointments = await unitOfWork.Appointments.GetAppointmentsByCustomer(customerId);
                return Ok(appointments);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sorun oluştu");
            }
        }

        [HttpGet("GetAppointment")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await unitOfWork.Appointments.Get(id);
            if (appointment == null)
            {
                return NotFound("Appointment Bulunamadı");
            }
            return appointment;
        }

        /* END APPOINTMENT REQUESTS */
        /*------------------------------------------------------------------------------------------------------------------------------------*/
        /* CONTACT REQUESTS */

        [HttpGet("GetContact")]
        public async Task<ActionResult<Contact>> GetContact(int id)
        {
            var contact = await unitOfWork.Contacts.Get(id);
            if (contact == null)
            {
                return NotFound("Contact Bulunamadı");
            }
            return contact;
        }

        [HttpPost("AddContactRequest")]
        public async Task<ActionResult<Contact>> AddContactRequest([FromForm]Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await unitOfWork.Contacts.Add(contact);
                    int result = await unitOfWork.Complete();

                    return CreatedAtAction("GetContact", new { id = contact.Id }, contact);
                }
                catch (Exception)
                {
                    return BadRequest("Başarısız");
                }

            }
            return BadRequest("Lütfen tüm alanları doldurunuz!");
        }

        /* END CONTACT REQUESTS */
        /*------------------------------------------------------------------------------------------------------------------------------------*/
        /* PHOTO REQUESTS */

        [HttpGet("GetPhotos")]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(int appointmentId, string customerId)
        {
            var currentUser = await userManager.FindByIdAsync(customerId);
            if (currentUser == null)
            {
                return NotFound("Kullanıcı bulunamadı");
            }

            try
            {
                if (await unitOfWork.Appointments.Get(appointmentId) == null)
                {
                    return NotFound("Appointment Bulunamadı");
                }

                var photos = await unitOfWork.Photos.GetPhotos(appointmentId, customerId);
                return Ok(photos);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sorun oluştu");
            }
        }

        /* END PHOTO REQUESTS */
        /*------------------------------------------------------------------------------------------------------------------------------------*/
        /* PHOTO SHOOT TYPE REQUESTS */

        [Authorize]
        [HttpGet("GetPhotoShootTypes")]
        public async Task<ActionResult<IEnumerable<ShootType>>> GetPhotoShootTypes()
        {
            try
            {
                var shootTypes = await unitOfWork.ShootTypes.Find(s => s.IsActive == true);
                return Ok(shootTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }
        }

        [HttpGet("GetPhotoShootType")]
        public async Task<ActionResult<ShootType>> GetPhotoShootType(int id)
        {
            try
            {
                var shootType = await unitOfWork.ShootTypes.Get(id);
                if (shootType == null)
                    return NotFound("Bulunamadı");
                return Ok(shootType);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }
        }

        /* PHOTO SHOOT TYPE REQUESTS */
        /*------------------------------------------------------------------------------------------------------------------------------------*/
        /* PRIVATE REQUESTS */
        [HttpPost("UpdateScheduleIsEmptyField")]
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

        /*------------------------------------------------------------------------------------------------------------------------------------*/
        /* END PRIVATE REQUESTS */

        [HttpPost("Login")]
        public async Task<object> Login([FromForm] LoginDto model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                return await GenerateJwtToken(model.Email, appUser);
            }

            throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        private async Task<object> GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                configuration["JwtIssuer"],
                configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
    public class LoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

    }
}