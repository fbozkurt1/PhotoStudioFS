using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoStudioFS.Data;
using PhotoStudioFS.Models;

namespace PhotoStudioFS.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class AdminController : BaseController
    {

        public AdminController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context)
           : base(context, userManager, signInManager)
        {
        }

        public async Task<IActionResult> Index()
        {
            var pendingAppointments = await unitOfWork.Appointments.GetAppointmentsByState(0);
            int totalDoneAppointment = await unitOfWork.Appointments.GetCountAppointmentsByState(2);
            int totalAppointmentRequest = await unitOfWork.Appointments.GetCountAppointmentsByIsApproved(0);

            ViewData["TotalPendingAppointment"] = String.Format("{0:n0}", pendingAppointments.Count());
            ViewData["TotalDoneAppointment"] = String.Format("{0:n0}", totalDoneAppointment);
            ViewData["TotalAppointmentRequest"] = String.Format("{0:n0}", totalAppointmentRequest);
            ViewData["TotalFeedbackMessage"] = String.Format("{0:n0}", 0);
            ViewData["TotalContactRequest"] = String.Format("{0:n0}", 6);

            return View(pendingAppointments);
        }
    }
}