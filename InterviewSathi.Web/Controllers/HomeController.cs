using InterviewSathi.Web.Data;
using InterviewSathi.Web.Migrations;
using InterviewSathi.Web.Models;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.Services;
using InterviewSathi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;

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
            return View();
        }

        public async Task<IActionResult> Experts(string? searchName = null, string? searchSkill = null)
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

            var searchedExperts = interviewerUsers
                .Where(x => (searchName == null || x.UserName.Contains(searchName, StringComparison.OrdinalIgnoreCase)))
                .Where(x => (searchSkill == null || x.Skills.Any(y => y.Skill.NameOfSkill.Contains(searchSkill, StringComparison.OrdinalIgnoreCase))))
            .ToList();

            var skills = _context.Skills.ToList();
            ViewBag.skillList = new SelectList(skills, nameof(Skill.NameOfSkill), nameof(Skill.NameOfSkill));

            return View(searchedExperts);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> NewExpert()
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
                DocURL = user.DocURL,
                Email = user.Email,
                IsVerified = user.IsVerified,
            }).ToList();

            return View(interviewerUsers);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            return View(await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == id));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, string email, string message, int flexRadioDefault)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (flexRadioDefault == 1)
            {
                user.IsVerified = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                EmailService.SendMail(email, "InterviewSathi - Your Account Verification", $"We have your account verified " +
                $"Now Enjoy and help and lets grow togther. </br> {message}"
                );
            }
            else 
            {
                user.IsVerified = false;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                EmailService.SendMail(email, "InterviewSathi - Your Account Verification", $"We have your account verified " +
                $"Please Provide a valid documnet </br> {message}"
                );
            }
            return RedirectToAction("NewExpert", "Home");
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
