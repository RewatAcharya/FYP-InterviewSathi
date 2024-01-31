using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.ViewModels;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = applicationDbContext;
            _roleManager = roleManager;
            _userManager = userManager;
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

        public async Task<IActionResult> Experts()
        {
            // Getting the "Interviewer" role
            var interviewerRole = await _roleManager.FindByNameAsync("Interviewer");

            if (interviewerRole == null)
            {
                return NotFound("Role not found");
            }

            // Getting users with the "Interviewer" role
            var usersWithInterviewerRole = await _userManager.GetUsersInRoleAsync("Interviewer");

            var interviewerUsers = usersWithInterviewerRole.Select(user => new ExpertVM
            {
                UserId = user.Id,
                UserName = user.Name,
                Profile = user.ProfileURL,
                Skills = _context.UserSkills.Where(x => x.UserId == user.Id).Include(x => x.Skill).ToList()
            }).ToList();

            return View(interviewerUsers);
        }

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
