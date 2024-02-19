using InterviewSathi.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;

namespace InterviewSathi.Web.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string id)
        {
            var friends = _context.Friends.Include(friend => friend.SendingTo).Include(friend => friend.SendingBy)
                .Where(x => x.SentTo == id || x.SentBy == id).ToList();
            return View(friends);
        }

        public IActionResult Chat(string id)
        {
            var receiverId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();

            var messages = _context.PrivateMessages
                .Where(m => (m.SenderId == id && m.ReceiverId == receiverId) || (m.SenderId == receiverId && m.ReceiverId == id))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            ViewBag.Messages = messages;

            return PartialView(_context.ApplicationUsers.First(x => x.Id == id));
        }
    }
}
