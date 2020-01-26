using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PhotoStudioFS.Core;
using PhotoStudioFS.Data;
using PhotoStudioFS.Models;

namespace PhotoStudioFS.Controllers
{
    public class BaseController : Controller
    {
        protected UnitOfWork unitOfWork { get; private set; }
        protected photostudioContext context { get; private set; }
        protected UserManager<User> userManager { get; private set; }
        protected SignInManager<User> signInManager { get; private set; }
        public BaseController(
            photostudioContext _context,
            UserManager<User> _userManager,
            SignInManager<User> _signInManager)
        {
            context = _context;
            unitOfWork = new UnitOfWork(_context);
            userManager = _userManager;
            signInManager = _signInManager;
        }
    }
}