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
            await Clients.User(receiverId).SendAsync("ReceiveVideoOffer", message);
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

        //public async Task JoinRoom(string roomName)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        //    Console.WriteLine($"User {Context.User.Identity.Name} joined room {roomName}");
        //}

        //public async Task SendSignal(string userToId, string signal)
        //{
        //    try
        //    {
        //        Console.WriteLine($"Sending signal to {userToId}: {signal}");

        //        var userFrom = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        //        var callerName = Context.User.Identity.Name;

        //        await Clients.User(userToId).SendAsync("ReceiveSignal", userFrom, signal);
        //        await Clients.User(userToId).SendAsync("ReceiveCall", callerName);

        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception using a more detailed logging system
        //        Console.Error.WriteLine($"Error in SendSignal: {ex.Message}");
        //        throw; // Rethrow the exception to preserve the original behavior
        //    }
        //}
    }
}
