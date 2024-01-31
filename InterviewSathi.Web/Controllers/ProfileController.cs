using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.Models.Entities.BlogsEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Drawing;
using System.Security.Claims;

namespace InterviewSathi.Web.Controllers
{
    public class ProfileController : Controller
    {
        ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _env;


        public ProfileController(ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [Authorize]
        public IActionResult Index(string username)
        {
            string userId = _dbContext.ApplicationUsers.FirstOrDefault(x => x.Email == username).Id;
            var blogs = _dbContext.Blogs.Where(x => x.PostedBy == userId).ToList();
            ViewBag.Blogs = blogs;
            ViewBag.Skills = _dbContext.UserSkills.Where(x => x.UserId == userId).Include(x => x.Skill).ToList();
            return View(_dbContext.ApplicationUsers.FirstOrDefault(x => x.Email == username));
        }

        [Authorize]
        public IActionResult UserProfile(string Id)
        {
            var blogs = _dbContext.Blogs.Where(x => x.PostedBy == Id).ToList();
            ViewBag.Blogs = blogs;
            ViewBag.Skills = _dbContext.UserSkills.Where(x => x.UserId == Id).Include(x => x.Skill).ToList();

            string? myId = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();

            ViewBag.FriendId = _dbContext.Friends.FirstOrDefault(x => (x.SentTo == Id && x.SentBy == myId) || (x.SentBy == Id && x.SentTo == myId))?.Id;

            ViewBag.Count = _dbContext.Friends
                .Where(x => (x.SentTo == Id && x.SentBy == myId) || (x.SentBy == Id && x.SentTo == myId)).Count();

            return View(_dbContext.ApplicationUsers.FirstOrDefault(x => x.Id == Id));
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(string? id)
        {
            if (id == null) 
            { 
                return NotFound(); 
            }
            var user = await _dbContext.ApplicationUsers.FindAsync(id);
            if (user == null) 
            {
                return NotFound();
            }
            return PartialView(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(ApplicationUser appUser)
        {
            ApplicationUser? applicationUser = await _dbContext.ApplicationUsers.FindAsync(appUser.Id);
            
            if (ModelState.IsValid && applicationUser != null)
            {
                if (appUser.ProfileUpload != null)
                {
                    string profileName = Guid.NewGuid() + Path.GetExtension(appUser.ProfileUpload.FileName);
                    string profilePath = Path.Combine(_env.WebRootPath, @"Images\Profiles\", profileName);
                    using (FileStream stream = new FileStream(profilePath, FileMode.Create))
                    {
                        appUser.ProfileUpload.CopyTo(stream);
                    }
                    applicationUser.ProfileURL = profileName;
                }
                if (appUser.CoverUpload != null)
                {
                    string coverName = Guid.NewGuid() + Path.GetExtension(appUser.CoverUpload.FileName);
                    string coverPath = Path.Combine(_env.WebRootPath, @"Images\Covers\", coverName);
                    using (FileStream stream = new FileStream(coverPath, FileMode.Create))
                    {
                        appUser.CoverUpload.CopyTo(stream);
                    }
                    applicationUser.CoverURL = coverName;
                }
                applicationUser.Name = appUser.Name;
                applicationUser.Bio = appUser.Bio;

                _dbContext.Update(applicationUser);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index", "Profile", new { username = applicationUser.UserName });
            }
            return View(appUser);
        }
    }
}
