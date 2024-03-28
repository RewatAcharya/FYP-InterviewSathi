using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InterviewSathi.Web.Controllers
{
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MeetingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string id)
        {
            List<Meeting> meeting = await _context.Meetings.Where(x => (x.SentTo == id) || (x.SentBy == id)).Include(x => x.SendingTo).Include(x => x.SendingBy).ToListAsync();
            return View(meeting);
        }

        public IActionResult Create(string id)
        {
            ViewBag.SelectedUser = _context.ApplicationUsers.FirstOrDefault(x => x.Id == id);
            return View();
        }

        private async Task CreateMeetingRequestAsync(DateOnly date, TimeOnly time, string sentBy, string sentTo, string type)
        {
            Meeting meeting = new Meeting()
            {
                MeetingDate = date,
                MeetingTime = time,
                InterviewType = type,
                SentBy = sentBy,
                SentTo = sentTo,
            };

            _context.Add(meeting);
            await _context.SaveChangesAsync();

            string? email = _context.ApplicationUsers.First(x => x.Id == meeting.SentTo).Email;
            string? name = _context.ApplicationUsers.First(x => x.Id == meeting.SentBy).Name;

            Notification notification = new()
            {
                SentBy = meeting.SentBy,
                SentTo = meeting.SentTo,
                Type = NotificationType.Meeting.ToString(),
                Content = $"You have a new meeting request from {name}!!",
                CreatedAt = DateTime.UtcNow,
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            EmailService.SendMail(email, "InterviewSathi - New Meeting Request", $"You have a new meeting request from {name} " +
                $"schedule for Date: {date} and Time: {time}. " +
                $"Go to your profile to see or Click the link below to view more detail: </br>" +
                $"<a href=\"https://localhost:7236/Meeting/Index/\" + {meeting.SentTo}\">Link to follow</a>");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Meeting meeting)
        {
            var transaction = _context.Database.BeginTransaction();

            try
            {
                transaction.CreateSavepoint("BeforeAddingMeeting");
                await CreateMeetingRequestAsync(meeting.MeetingDate, meeting.MeetingTime, meeting.SentBy, meeting.SentTo, meeting.InterviewType);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.RollbackToSavepoint("BeforeAddingMeeting");
                Console.WriteLine(ex.ToString());
            }
            TempData["success"] = "Successfully scheduled";
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
            var meeting = await _context.Meetings.FindAsync(id);
            string sentName = _context.ApplicationUsers.FirstOrDefault(x => x.Id == meeting.SentTo)?.Name;
            string senderName = _context.ApplicationUsers.FirstOrDefault(x => x.Id == meeting.SentBy)?.Name;
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

            return RedirectToAction("Index", "Chat", new { id = rr.RatedBy });
        }
    }
}
