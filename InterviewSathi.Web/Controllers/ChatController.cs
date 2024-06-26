﻿using InterviewSathi.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index(string id, string? chat = null)
        {
            // id is the user ID for whom you want to find recent chat users
            var recentChatUsers = _context.PrivateMessages
                .Where(msg => msg.SenderId == id || msg.ReceiverId == id)
                .GroupBy(msg => msg.SenderId == id ? msg.ReceiverId : msg.SenderId)
                .Select(group => group.Key)
                .Distinct()
                .Select(userId => _context.Users.FirstOrDefault(u => u.Id == userId))
                .ToList();

            var firstUser = recentChatUsers.FirstOrDefault();

            if (chat != null)
            {
                ViewBag.ChatWith = chat;
            }
            else
            {
                ViewBag.ChatWith = firstUser?.Id;
            }

            return View(recentChatUsers);
        }

        public IActionResult Chat(string id)
        {
            var receiverId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();

            var messages = _context.PrivateMessages
                .Where(m => (m.SenderId == id && m.ReceiverId == receiverId) || (m.SenderId == receiverId && m.ReceiverId == id))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            ViewBag.Messages = messages;

            if (id != null)
            {
                return PartialView(_context.ApplicationUsers.First(x => x.Id == id));
            }
            else
            {
                return Index(receiverId);
            }
        }

        [HttpDelete("/Chat/delete-message/{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var message = await _context.PrivateMessages.FindAsync(id);
            if (message != null)
            {
                _context.PrivateMessages.Remove(message);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Message deleted successfully" });
            }

            return NotFound(new { error = "Message not found" });
        }
    }
}
