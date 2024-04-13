using InterviewSathi.Web.Data;
using InterviewSathi.Web.Migrations;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;

namespace InterviewSathi.Web.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ContactUsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _context.ContactUs.ToListAsync();
            return View(result);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var result = await _context.ContactUs.FirstOrDefaultAsync(x => x.Id == id);
            return View(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(ContactUs contactUs, string emailMessage)
        {
            ContactUs result = await _context.ContactUs.FirstOrDefaultAsync(x => x.Id == contactUs.Id);

            if (result != null)
            {
                result.IsViewed = true;
                _context.ContactUs.Update(result);
                await _context.SaveChangesAsync();
                if (emailMessage != null)
                {
                    EmailService.SendMail(contactUs.Email, "InterviewSathi - Info from Contact Us", $"{emailMessage}");
                }

                TempData["success"] = "Successfully send the information to the user.";
                return RedirectToAction("Index", "ContactUs");
            }
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContactUs contactUs)
        {
            if(User.FindFirstValue(ClaimTypes.NameIdentifier) != null)
            {
                contactUs.Sender = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
            }

            var result = await _context.ContactUs.AddAsync(contactUs);
            await _context.SaveChangesAsync();
            TempData["success"] = "Your submission is successful. We'll get to you soon!!";
            return RedirectToAction("Index", "Home");
        }
    }
}
