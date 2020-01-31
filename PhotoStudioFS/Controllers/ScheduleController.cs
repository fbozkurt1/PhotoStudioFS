using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoStudioFS.Data;
using PhotoStudioFS.Models;

namespace PhotoStudioFS.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ScheduleController : BaseController
    {
        public ScheduleController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context)
               : base(context, userManager, signInManager)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
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
                schedules = await unitOfWork.Schedules.GetSchedules(dtStart, dtEnd);
            else
                schedules = await unitOfWork.Schedules.GetSchedulesByPhotoType(dtStart, dtEnd, photoType);

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
                        title = schedule.title,
                        photoShootType = schedule.ShootType.Name,
                        photoShootTypeId = schedule.ShootType.Id,
                        color = schedule.isEmpty == true ? "#6ced15" : "#ed4734"
                    });
                }
            }
            return Json(schedulesView);
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule(ScheduleView scheduleView)
        {
            try
            {
                if (Convert.ToDateTime(scheduleView.start) >= Convert.ToDateTime(scheduleView.end))
                {
                    return BadRequest("Başlangıç saati bitiş saatinden büyük veya aynı olamaz!");
                }
                if (string.IsNullOrEmpty(scheduleView.title) || string.IsNullOrWhiteSpace(scheduleView.title))
                {
                    return BadRequest("Gönderilen veri hatalı!");
                }
                var start = Convert.ToDateTime(scheduleView.start);
                var end = Convert.ToDateTime(scheduleView.end);

                var schedule = new Schedule()
                {
                    allDay = scheduleView.allDay,
                    isEmpty = scheduleView.isEmpty,
                    start = start,
                    end = end,
                    title = scheduleView.title,
                    ShootTypeId = scheduleView.photoShootTypeId
                };
                await unitOfWork.Schedules.Add(schedule);
                await unitOfWork.Complete();
            }
            catch (Exception)
            {
                return BadRequest("Randevu Takvimi eklenirken bir sorun oluştu! " +
                    "Lütfen saatleri uygun şekilde girip tekrar deneyiniz.");
            }


            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(int id)
        {

            var schedule = await unitOfWork.Schedules.Get(id);
            if (schedule == null)
            {
                return NotFound();
            }

            unitOfWork.Schedules.Remove(schedule);
            await unitOfWork.Complete();
            return Ok();
        }

        public async Task<IActionResult> UpdateIsEmptyField(int id, bool isEmpty)
        {
            var schedule = await unitOfWork.Schedules.Get(id);
            if (schedule == null)
                return NotFound("Schedule Bulunamadı");

            schedule.isEmpty = isEmpty;
            unitOfWork.Schedules.Update(schedule);
            try
            {
                await unitOfWork.Complete();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
