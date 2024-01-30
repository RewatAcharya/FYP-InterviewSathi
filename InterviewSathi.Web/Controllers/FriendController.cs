using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.Models.Entities.BlogsEntity;
using InterviewSathi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net;
using System.Security.Policy;

namespace InterviewSathi.Web.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FriendController(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public IActionResult Index(string Id)
        {
            return View(_context.Friends.Include(friend => friend.SendingTo).Include(friend => friend.SendingBy)
                .Where(x => x.SentTo == Id || x.SentBy == Id).ToList()
                );
        }

        public IActionResult Create(string id)
        {
            // Getting the list of user IDs who are already friends or have pending friend requests
            var excludedUserIds = _context.Friends
                .Where(f => f.SentTo == id || f.SentBy == id)
                .Select(f => f.SentTo == id ? f.SentBy : f.SentTo)
                .ToList();

            // Adding the current user ID to the excluded list to avoid showing them in the list
            excludedUserIds.Add(id);

            // Getting the list of users excluding those who are already friends or have pending requests
            List<ApplicationUser> users = _context.ApplicationUsers
                .Where(x => !excludedUserIds.Contains(x.Id))
                .ToList();

            ViewBag.Users = users;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Friend friend)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                transaction.CreateSavepoint("BeforeAddingFriend");

                friend.Id = Guid.NewGuid().ToString();
                _context.Add(friend);
                var result = await _context.SaveChangesAsync();

                string? email = _context.ApplicationUsers.Where(x => x.Id == friend.SentTo).First().Email;
                string? name = _context.ApplicationUsers.Where(x => x.Id == friend.SentBy).First().Name;

                Notification notification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    SentBy = friend.SentBy,
                    SentTo = friend.SentTo,
                    Type = "Friend",
                    Content = $"You have a new friend request from {name}!!",
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                EmailService.SendMail(email, "InterviewSathi - New friend Request", $"You have a new friend request from {name}. " +
                    $"Click the link below to view: </br>" +
                    $"<a href=\"https://localhost:7236/Friend/Index/\" + {friend.SentTo}\">Link to follow</a>");

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.RollbackToSavepoint("BeforeAddingFriend");
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("Index", "Friend", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }

        public async Task<IActionResult> Edit(string id)
        {
            var friend = await _context.Friends.FindAsync(id);
            if (friend != null)
            {
                friend.Accepted = true;
                _context.Friends.Update(friend);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Friend", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }

        public async Task<IActionResult> Delete(string id)
        {
            var friend = await _context.Friends.FindAsync(id);
            if (friend != null)
            {
                _context.Friends.Remove(friend);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Friend", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }
    }

}
