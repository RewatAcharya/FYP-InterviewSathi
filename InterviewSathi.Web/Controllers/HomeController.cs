using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models;
using InterviewSathi.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace InterviewSathi.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = applicationDbContext;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
                _roleManager.CreateAsync(new IdentityRole("Interviewer")).Wait();
                _roleManager.CreateAsync(new IdentityRole("Interviewee")).Wait();
            }
            return View();
        }

        //public IActionResult Experts()
        //{
        //    // Assuming _roleManager is an instance of RoleManager<IdentityRole>
        //    var interviewerRoleId = _roleManager.Roles.SingleOrDefault(r => r.Name == "Interviewer")?.Id;

        //    if (interviewerRoleId != null)
        //    {
        //        // Retrieve users with the "Interviewer" role
        //        var experts = _context.ApplicationUsers
        //            .Where(user => user.UserRoles.Any(ur => ur.RoleId == interviewerRoleId))
        //            .ToList();

        //        return View(experts);
        //    }
        //    else
        //    {
        //        // Handle the case where the "Interviewer" role doesn't exist
        //        // You might want to log an error or handle it appropriately
        //        return RedirectToAction("Error");
        //    }
        //}



        [Authorize]
        public IActionResult NewsFeed()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
