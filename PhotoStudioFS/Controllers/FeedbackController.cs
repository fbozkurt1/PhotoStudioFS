using System;
using System.Collections.Generic;
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
    public class FeedbackController : BaseController
    {
        public FeedbackController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context)
                : base(context, userManager, signInManager)
        {
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}