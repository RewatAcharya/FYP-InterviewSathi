using InterviewSathi.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var notification = _context.Notifications.Where(x => x.SentTo == id).Include(x => x.SendingBy).ToList();
            return View(notification);
        }
    }
}
