using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.Models.Entities.BlogsEntity;
using InterviewSathi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InterviewSathi.Web.Controllers
{
    public class PlatformReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;


        public PlatformReviewController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var result = await _context.PlatformReviews.Include(x => x.User).ToListAsync();
            return View(result);
        }

        [Authorize]
        public IActionResult Create(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(PlatformReview platformReview)
        {
            if (platformReview == null)
            {
                TempData["error"] = "Your review was not sent. Please try again.";
                return View(new { id = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString() });
            }
            if (platformReview.PicUpload != null)
            {
                string filename = Guid.NewGuid() + Path.GetExtension(platformReview.PicUpload.FileName);
                string imgpath = Path.Combine(_env.WebRootPath, "Images/PlaformReview/", filename);
                using (FileStream streamread = new FileStream(imgpath, FileMode.Create))
                {
                    platformReview.PicUpload.CopyTo(streamread);
                }
                platformReview.PicURL = filename;
            }

            await _context.PlatformReviews.AddAsync(platformReview);
            await _context.SaveChangesAsync();
            TempData["success"] = "Your review has successfully received. Thank You.";
            return RedirectToAction("Index", "Profile", new { id = User?.FindFirstValue(ClaimTypes.NameIdentifier).ToString() });
        }

        //edit by admin to change status to read

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var result = await _context.PlatformReviews.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            return View(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(PlatformReview review, string emailMessage)
        {
            PlatformReview result = await _context.PlatformReviews.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == review.Id);

            if (result != null)
            {
                result.Status = true;
                _context.PlatformReviews.Update(result);
                await _context.SaveChangesAsync();
                EmailService.SendMail(result.User.Email, "InterviewSathi - Reply from Suggestions", $"{emailMessage}");

                TempData["success"] = "Successfully send the information to the user.";
                return RedirectToAction("Index", "PlatformReview");
            }
            return RedirectToAction("Edit", "PlatformReview", new { id = review.Id });
        }

        //no delete as it is important

    }
}
