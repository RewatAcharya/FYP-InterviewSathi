using InterviewSathi.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            return PartialView(_context.ApplicationUsers.First(x => x.Id == id));
        }
    }
}
