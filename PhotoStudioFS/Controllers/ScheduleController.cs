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
        public async Task<IActionResult> Index()
        {
            var shootTypes = await unitOfWork.ShootTypes.Find(s => s.IsActive == true);
            return View(shootTypes);
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

                var shootType = await unitOfWork.ShootTypes.Get(scheduleView.photoShootTypeId);
                if (shootType == null)
                {
                    return BadRequest("Çekim Türü bulunamadı. Lütfen tekrar deneyiniz!");
                }

                var start = Convert.ToDateTime(scheduleView.start);
                var end = Convert.ToDateTime(scheduleView.end);

                var schedule = new Schedule()
                {
                    allDay = scheduleView.allDay,
                    isEmpty = scheduleView.isEmpty,
                    start = start,
                    end = end,
                    ShootTypeId = shootType.Id
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
