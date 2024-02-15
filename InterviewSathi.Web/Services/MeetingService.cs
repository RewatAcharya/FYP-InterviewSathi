using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewSathi.Web.Services
{
    public class MeetingService
    {
        private readonly ApplicationDbContext _context;

        public MeetingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Meeting> GetUpcomingMeetings(DateTime currentDateTime, DateTime targetDateTime)
        {
            var allMeetings = _context.Meetings
                .Include(x => x.SendingBy)
                .Include(x => x.SendingTo)
                .ToList();

            var upcomingMeetings = allMeetings
                .Where(m => m.MeetingDate.ToDateTime(m.MeetingTime) > currentDateTime && m.MeetingDate.ToDateTime(m.MeetingTime) <= targetDateTime && m.Status == true && m.MeetingStatus == false)
                .ToList();

            foreach(var m in upcomingMeetings)
            {
                m.MeetingStatus = true;
                _context.Meetings.Update(m);
                _context.SaveChangesAsync();
            }

            return upcomingMeetings;
        }
    }
}
