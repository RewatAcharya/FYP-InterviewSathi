using InterviewSathi.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InterviewSathi.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string id)
        {
            var notification = _context.Notifications.Where(x => x.SentTo == id || x.SentTo == null).Include(x => x.SendingBy).OrderByDescending(x => x.CreatedAt).ToList();
            return View(notification);
        }

        public async Task<IActionResult> Delete(string Id)
        {
            var notification = await _context.Notifications.FindAsync(Id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Notification", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() });
        }
    }
}
