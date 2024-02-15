using InterviewSathi.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace InterviewSathi.Web.Services
{
    public class EmailNotificationService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public EmailNotificationService(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
        }
    

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1)); // adjust the interval as needed
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var meetingService = scope.ServiceProvider.GetRequiredService<MeetingService>();

                DateTime currentDateTime = DateTime.Now;
                DateTime targetDateTime = currentDateTime.AddMinutes(15);

                var upcomingMeetings = meetingService.GetUpcomingMeetings(currentDateTime, targetDateTime);


                foreach (var meeting in upcomingMeetings)
                {
                    EmailService.SendMail(meeting.SendingTo.UserName, "Upcoming Interview", "Your interview is scheduled in 15 minutes.");
                    EmailService.SendMail(meeting.SendingBy.UserName, "Upcoming Interview", "Your interview is scheduled in 15 minutes.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
