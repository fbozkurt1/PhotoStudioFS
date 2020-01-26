using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoStudioFS.Data;
using PhotoStudioFS.Models;

namespace PhotoStudioFS.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(UserManager<User> userManager, SignInManager<User> signInManager, photostudioContext context)
               : base(context, userManager, signInManager)
        {
        }

        public IActionResult Index()
        {
            ViewBag.Contact = new Contact();
            var cont = unitOfWork.Contacts.Get(1);
            ViewBag.contact = cont;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
