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
using InterviewSathi.Web.Models;
using InterviewSathi.Web.Migrations;
using Friend = InterviewSathi.Web.Models.Entities.Friend;

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

        public async Task<IActionResult> Index(string Id)
        {
            return View(await _context.Friends.Include(friend => friend.SendingTo).Include(friend => friend.SendingBy)
                .Where(x => x.SentTo == Id || x.SentBy == Id).ToListAsync()
                );
        }

        public async Task<IActionResult> Create(string id, string searchUser)
        {
            // Getting the list of user IDs who are already friends or have pending friend requests
            var excludedUserIds = await _context.Friends
                .Where(f => f.SentTo == id || f.SentBy == id)
                .Select(f => f.SentTo == id ? f.SentBy : f.SentTo)
                .ToListAsync();

            // Adding the current user ID to the excluded list to avoid showing them in the list
            excludedUserIds.Add(id);

            // Getting the list of users excluding those who are already friends or have pending requests
            List<ApplicationUser> users = await _context.ApplicationUsers
                .Where(x => !excludedUserIds.Contains(x.Id))
                .Where(x => x.Name.Contains(searchUser) || searchUser == null)
                .ToListAsync();

            ViewBag.Users = users;
            return View();
        }

        private async Task CreateFriendRequestAsync(string sentBy, string sentTo)
        {
            Friend friend = new Friend()
            {
                SentBy = sentBy,
                SentTo = sentTo,
                Id = Guid.NewGuid().ToString()
            };

            await _context.AddAsync(friend);
            await _context.SaveChangesAsync();

            string? name = _context.ApplicationUsers.Where(x => x.Id == friend.SentBy).First().Name;

            Notification notification = new()
            {
                Id = Guid.NewGuid().ToString(),
                SentBy = friend.SentBy,
                SentTo = friend.SentTo,
                Type = NotificationType.Friend.ToString(),
                Content = $"You have a new friend request from {name}!!",
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Friend friend)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            string? name = _context.ApplicationUsers.Where(x => x.Id == friend.SentBy).First().Name;
            string? email = _context.ApplicationUsers.Where(x => x.Id == friend.SentTo).First().Email;

            try
            {
                transaction.CreateSavepoint("BeforeAddingFriend");
                await CreateFriendRequestAsync(friend.SentBy, friend.SentTo);

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                EmailService.SendMail(email, "InterviewSathi - New friend Request", $"You have a new friend request from {name}. " +
                    $"Click the link below to view: </br>" +
                    $"<a href=\"{domain}Friend/Index/{friend.SentTo}\">Link to follow</a>");

                transaction.Commit();
                TempData["success"] = "Request Successful";
            }
            catch (Exception)
            {
                TempData["error"] = "Request Failed";
                await transaction.RollbackToSavepointAsync("BeforeAddingFriend");
            }

            return RedirectToAction("Index", "Friend", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFriend(string sendBy, string sendTo)
        {
            var transaction = _context.Database.BeginTransaction();
            string? name = _context.ApplicationUsers.Where(x => x.Id == sendBy).First().Name;
            string? email = _context.ApplicationUsers.Where(x => x.Id == sendTo).First().Email;

            try
            {
                transaction.CreateSavepoint("BeforeAddingFriend");
                await CreateFriendRequestAsync(sendBy, sendTo);

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                EmailService.SendMail(email, "InterviewSathi - New friend Request", $"You have a new friend request from {name}. " +
                    $"Click the link below to view: </br>" +
                    $"<a href=\"{domain}Friend/Index/{sendTo}\">Link to follow</a>");

                transaction.Commit();
                TempData["success"] = "Request Successful";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Request Failed";
                await transaction.RollbackToSavepointAsync("BeforeAddingFriend");
            }

            return RedirectToAction("UserProfile", "Profile", new { id = sendTo });
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
            string? name = _context.ApplicationUsers.Where(x => x.Id == friend.SentTo).First().Name;

            Notification notification = new()
            {
                Id = Guid.NewGuid().ToString(),
                SentBy = friend.SentTo,
                SentTo = friend.SentBy,
                Type = NotificationType.Friend.ToString(),
                Content = $"Your friend request is accepted by {name}!!",
                CreatedAt = DateTime.UtcNow,
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();
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

        public async Task<IActionResult> DeleteFriend(string id)
        {
            Friend friend = await _context.Friends.FindAsync(id);
            string friendId = friend.SentBy;
            if (friend != null)
            {
                _context.Friends.Remove(friend);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("UserProfile", "Profile", new { id = friendId });
        }
    }

}
