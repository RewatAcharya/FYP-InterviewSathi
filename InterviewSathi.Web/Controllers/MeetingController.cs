using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.Services;
using InterviewSathi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InterviewSathi.Web.Controllers
{
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MeetingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize]
        public async Task<IActionResult> Index(string id, int? month)
        {
            month ??= DateTime.Today.Month;
            List<Meeting> meeting = await _context.Meetings
                .Where(x => (x.SentTo == id) || (x.SentBy == id))
                .Include(x => x.SendingTo)
                .Include(x => x.SendingBy)
                .ToListAsync();
            if (month.HasValue)
            {
                meeting = meeting.Where(m => m.MeetingDate.Month == month.Value && m.MeetingDate.Year == DateTime.UtcNow.Year).ToList();
            }
            return View(meeting);
        }


        public async Task<IActionResult> Create(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var firstRole = roles.FirstOrDefault();

            ViewBag.SelectedUser = user;
            ViewBag.FirstRole = firstRole;

            return View();
        }

        private async Task CreateMeetingRequestAsync(DateOnly date, TimeOnly time, string sentBy, string sentTo, string type, bool paidStatus)
        {
            Meeting meeting = new Meeting()
            {
                MeetingDate = date,
                MeetingTime = time,
                InterviewType = type,
                SentBy = sentBy,
                SentTo = sentTo,
                MeetingType = paidStatus
            };

            await _context.AddAsync(meeting);
            await _context.SaveChangesAsync();

            string? email = _context.ApplicationUsers
                .First(x => x.Id == meeting.SentTo).Email;
            string? name = _context.ApplicationUsers
                .First(x => x.Id == meeting.SentBy).Name;

            Notification notification = new()
            {
                SentBy = meeting.SentBy,
                SentTo = meeting.SentTo,
                Type = NotificationType.Meeting.ToString(),
                Content = $"You have a new meeting request from {name}!!",
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Meeting meeting)
        {
            if (meeting.MeetingDate.ToDateTime(meeting.MeetingTime) <= DateTime.Now)
            {
                TempData["error"] = "Please select a date carefully.";
                return RedirectToAction("Create", "Meeting", new { id = meeting.SentTo });
            }


            var transaction = _context.Database.BeginTransaction();

            try
            {
                transaction.CreateSavepoint("BeforeAddingMeeting");
                await CreateMeetingRequestAsync(meeting.MeetingDate, meeting.MeetingTime, meeting.SentBy, meeting.SentTo, meeting.InterviewType, meeting.MeetingType);

                string? email = _context.ApplicationUsers.First(x => x.Id == meeting.SentTo).Email;
                string? name = _context.ApplicationUsers.First(x => x.Id == meeting.SentBy).Name;
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var successUrl = domain + $"meeting/index/{meeting.SentBy}";
                var cancelUrl = domain + $"meeting/create/{meeting.SentTo}";

                if (meeting.MeetingType == true)
                {
                    var options = new SessionCreateOptions()
                    {
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                        SuccessUrl = successUrl,
                        CancelUrl = cancelUrl,
                    };

                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(6 * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = email,
                            },
                        },
                        Quantity = 1,
                    });

                    var service = new SessionService();
                    Session session = service.Create(options);

                    Response.Headers.Add("Location", session.Url);
                    transaction.Commit();

                    EmailService.SendMail(email, "InterviewSathi - New Meeting Request", $"You have a new meeting request from {name} " +
        $"schedule for Date: {meeting.MeetingDate} and Time: {meeting.MeetingTime}. " +
        $"Go to your profile to see or Click the link below to view more detail: </br>" +
        $"<a href=\"{domain}Meeting/Index/{meeting.SentTo}\">Link to follow</a>");
                    return new StatusCodeResult(303);

                }
                else
                {
                    transaction.Commit();

                    EmailService.SendMail(email, "InterviewSathi - New Meeting Request", $"You have a new meeting request from {name} " +
        $"schedule for Date: {meeting.MeetingDate} and Time: {meeting.MeetingTime}. " +
        $"Go to your profile to see or Click the link below to view more detail: </br>" +
        $"<a href=\"{domain}Meeting/Index/{meeting.SentTo}\">Link to follow</a>");

                    return RedirectToAction("Index", "Meeting", new { id = meeting.SentBy });
                }
            }

            catch (Exception ex)
            {
                transaction.RollbackToSavepoint("BeforeAddingMeeting");
                TempData["error"] = "Try Again!!";
            }

            return RedirectToAction("Experts", "Home");
        }

        public async Task<IActionResult> Edit(string id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            return PartialView(meeting);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Meeting meeting)
        {
            if (meeting != null)
            {
                meeting.Status = true;
                _context.Meetings.Update(meeting);
            }
            await _context.SaveChangesAsync();
            string? name = _context.ApplicationUsers.Where(x => x.Id == meeting.SentTo).First().Name;

            Notification notification = new()
            {
                SentBy = meeting.SentTo,
                SentTo = meeting.SentBy,
                Type = NotificationType.Meeting.ToString(),
                Content = $"Your interview request is accepted by {name}!! for Date: {meeting.MeetingDate} and Time {meeting.MeetingTime}",
                CreatedAt = DateTime.UtcNow,
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return RedirectToAction("Index", "Meeting", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }


        public async Task<IActionResult> Delete(string id)
        {
            var deletingUser = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
            var meeting = await _context.Meetings.FindAsync(id);
            string? sentName = _context.ApplicationUsers.FirstOrDefault(x => x.Id == meeting.SentTo)?.Name;
            string? senderName = _context.ApplicationUsers.FirstOrDefault(x => x.Id == meeting.SentBy)?.Name;
            if (meeting.SentBy != deletingUser && User.IsInRole("Interviewer") && meeting.MeetingType == true)
            {
                TempData["error"] = "You can not remove paid interview request.";
                return RedirectToAction("Index", "Meeting", new { id = deletingUser });
            }

            if (meeting != null)
            {
                _context.Meetings.Remove(meeting);
            }
            await _context.SaveChangesAsync();


            Notification notification = new()
            {
                Id = Guid.NewGuid().ToString(),
                SentBy = meeting.SentTo,
                SentTo = meeting.SentBy,
                Type = NotificationType.Meeting.ToString(),
                CreatedAt = DateTime.UtcNow,
            };

            if (User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() == meeting.SentTo)
            {
                notification.SentBy = meeting.SentTo;
                notification.SentTo = meeting.SentBy;
                notification.Content = $"Your meeting request has been canceled by {sentName}!!";
            }
            else
            {
                notification.SentBy = meeting.SentBy;
                notification.SentTo = meeting.SentTo;
                notification.Content = $"Your meeting request has been canceled by {senderName}!!";
            }

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return RedirectToAction("Index", "Meeting", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }

        public async Task<IActionResult> Review(ReviewRating rr)
        {
            if (rr != null)
            {
                await _context.ReviewRatings.AddAsync(rr);
                await _context.SaveChangesAsync();
            }

            TempData["success"] = "Review successfully submitted";
            return RedirectToAction("Index", "Chat", new { id = rr.RatedBy });
        }
    }
}
