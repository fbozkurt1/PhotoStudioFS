﻿using System;
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
    [Route("[controller]")]
    [Authorize]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<ActionResult<Appointment>> AddAppointmentRequest([FromForm]AppointmentView appointmentView)
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

                try
                {


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
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            string userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrWhiteSpace(userEmail) || userEmail == null)
            {
                return NotFound("Token geçerli değil.");
            }
            try
            {
                var currentUser = await userManager.FindByEmailAsync(userEmail);
                if (currentUser == null)
                {
                    return NotFound("Giriş bilgileri geçerli değil.");
                }
                var appointments = await unitOfWork.Appointments.GetAppointmentsByCustomer(currentUser.Id);
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
        [AllowAnonymous]
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
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(int appointmentId)
        {
            string userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrWhiteSpace(userEmail) || userEmail == null)
            {
                return NotFound("Token geçerli değil.");
            }
            var currentUser = await userManager.FindByEmailAsync(userEmail);
            if (currentUser == null)
            {
                return NotFound("Kullanıcı bilgileri geçerli değil!");
            }

            try
            {
                if (await unitOfWork.Appointments.Get(appointmentId) == null)
                {
                    return NotFound("Appointment Bulunamadı");
                }

                var photos = await unitOfWork.Photos.GetPhotos(appointmentId, currentUser.Id);
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


        [HttpGet("GetPhotoShootTypes")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ShootType>>> GetPhotoShootTypes()
        {
            try
            {
                var shootTypes = await unitOfWork.ShootTypes.Find(s => s.IsActive == true);
                return Ok(shootTypes);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }
        }

        [HttpGet("GetPhotoShootType")]
        [AllowAnonymous]
        public async Task<ActionResult<ShootType>> GetPhotoShootType(int id)
        {
            try
            {
                var shootType = await unitOfWork.ShootTypes.Get(id);
                if (shootType == null)
                    return NotFound("Bulunamadı");
                return Ok(shootType);
            }
            catch (Exception)
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
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromForm] LoginView model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = userManager.Users.SingleOrDefault(u => u.Email == model.Email);
                var userView = new UserView()
                {
                    Token = await GenerateJwtToken(model.Email, appUser),
                    Email = appUser.Email,
                    Name = appUser.FullName,
                    Phone = appUser.PhoneNumber,
                    Success = true
                };
                return Ok(userView);

            }

            return NotFound(new UserView()
            {
                Email = model.Email,
                Success = false
            });
        }


        [HttpGet("CheckAuth")]
        [AllowAnonymous]
        public async Task<ActionResult> CheckAuth()
        {
            string tokenWithBearer = Request.Headers["Authorization"];

            var tokenWithBearerArray = tokenWithBearer.Split(' '); // takes bearer string and token to array
            if (tokenWithBearerArray.Count() != 2)
            {
                return BadRequest(new UserView()
                {
                    Success = false,
                    Token = "Token geçerli değil."
                });
            }
            var token = tokenWithBearerArray[1];
            string userId = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId) || userId == null)
            {
                return NotFound(new UserView()
                {
                    Success = false,
                    Token = "Token geçerli değil."
                });
            }

            var currentUser = await userManager.FindByIdAsync(userId);

            if (currentUser == null)
            {
                return NotFound(new UserView()
                {
                    Success = false,
                    Token = "Kullanıcı bilgileri geçerli değil."
                });
            }

            var userView = new UserView()
            {
                Email = currentUser.Email,
                Name = currentUser.FullName,
                Phone = currentUser.PhoneNumber,
                Success = true,
                Token = token
            };
            return Ok(userView);
        }

        private async Task<object> GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Name,user.Id),
                new Claim("userId",user.Id)
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
    public class LoginView
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

    }

    public class UserView
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool Success { get; set; }

        public object Token { get; set; }
    }
}