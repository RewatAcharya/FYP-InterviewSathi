using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;

namespace InterviewSathi.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task SendPrivateMessage(string receiverId, string message, string receiverName)
        {
            string? senderId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? senderName = _db.ApplicationUsers.FirstOrDefault(x => x.Id == senderId)?.Name;

            var users = new string[] { senderId, receiverId };
            string id = Guid.NewGuid().ToString();

            var newMessage = new PrivateMessage
            {
                Id = id,
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageContent = message,
            };

            _db.PrivateMessages.Add(newMessage);
            await _db.SaveChangesAsync();
            
            await Clients.Users(users).SendAsync("ReceivePrivateMessage", senderId, senderName, receiverId, message, id, receiverName);
        }

        public async Task SendVideoOffer(string receiverId, string message)
        {
            string? senderId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? senderName = _db.ApplicationUsers.FirstOrDefault(x => x.Id == senderId)?.Name;

            await Clients.User(receiverId).SendAsync("ReceiveVideoOffer", message, senderId, receiverId, senderName);
        }

        public async Task SendVideoAnswer(string callerUserId, string answer)
        {
            await Clients.User(callerUserId).SendAsync("ReceiveVideoAnswer", answer);
        }

        public async Task SendIceCandidate(string targetUserId, string iceCandidate)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveIceCandidate", iceCandidate, Context.User.FindFirstValue(ClaimTypes.NameIdentifier).ToString(), targetUserId);
        }

        public async Task SendIceCandidateToRemote(string targetUserId, string iceCandidate)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveIceCandidateFromLocal", iceCandidate, Context.User.FindFirstValue(ClaimTypes.NameIdentifier).ToString(), targetUserId);
        }

        public async Task SendHangUp(int num, string targetUserId)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveHangUp", num, Context.User.FindFirstValue(ClaimTypes.NameIdentifier).ToString(), targetUserId);
        }

    }
}
