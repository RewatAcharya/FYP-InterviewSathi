using InterviewSathi.Web.Data;
using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewSathi.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {

        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            int totalMeetingToday = await _context.Meetings
            .Where(x => x.MeetingDate == DateOnly.FromDateTime(DateTime.Today))
                .CountAsync();

            int totalNewContact = await _context.ContactUs.Where(x => x.IsViewed == false).CountAsync();
            int totalNewReviwes = await _context.PlatformReviews.Where(x => x.Status == false).CountAsync();

            ViewBag.TotalMeetingToday = totalMeetingToday;
            ViewBag.TotalNewContact = totalNewContact;
            ViewBag.TotalNewReviwes = totalNewReviwes;

            return View();
        }

        private async Task<(int totalUser, int userCurrentCount, int userPreviousCount)> GetUsers(string role)
        {
            var roles = await _roleManager.FindByNameAsync(role);

            // Getting users with the role
            var usersWithRole = await _userManager.GetUsersInRoleAsync(role);

            var users = usersWithRole.Select(user => new ExpertVM
            {
                UserId = user.Id,
                UserName = user.Name,
                Profile = user.ProfileURL,
                CreatedAt = user.CreatedAt,
                Skills = _context.UserSkills.Where(x => x.UserId == user.Id).Include(x => x.Skill).ToList()
            }).ToList();

            int total = users.Count();
            int current = users.Count(c => c.CreatedAt >= currentMonthStartDate && c.CreatedAt <= DateTime.Now);
            int previous = users.Count(c => c.CreatedAt >= previousMonthStartDate && c.CreatedAt < currentMonthStartDate);

            return (total, current, previous);
        }



        public async Task<IActionResult> GetTotalUserRadial()
        {
            var interviewers = await GetUsers("Interviewer");
            var interviewees = await GetUsers("Interviewee");

            int totalAppUser = (interviewers.totalUser + interviewees.totalUser);
            int totalAppUserCurrent = (interviewers.userCurrentCount + interviewees.userCurrentCount);
            int totalAppUserPrevious = (interviewers.userPreviousCount + interviewees.userPreviousCount);

            RadialChart radialChart = new RadialChart();
            int ratio = 100;
            if (totalAppUserPrevious > 0)
            {
                ratio = Convert.ToInt32((totalAppUserCurrent - totalAppUserPrevious) / totalAppUserPrevious * 100);
            }

            radialChart.TotalCount = totalAppUser;
            radialChart.TotalCurrent = totalAppUserCurrent;
            radialChart.hasIncreased = totalAppUserCurrent > totalAppUserPrevious;
            radialChart.Series = new int[] { ratio };

            return Json(radialChart);
        }

        public async Task<IActionResult> GetTotalInterviewerRadial()
        {
            var interviewers = await GetUsers("Interviewer");

            int totalAppUser = interviewers.totalUser;
            int totalAppUserCurrent = interviewers.userCurrentCount;
            int totalAppUserPrevious = interviewers.userPreviousCount;

            RadialChart radialChart = new RadialChart();
            int ratio = 100;
            if (totalAppUserPrevious > 0)
            {
                ratio = Convert.ToInt32((totalAppUserCurrent - totalAppUserPrevious) / totalAppUserPrevious * 100);
            }

            radialChart.TotalCount = totalAppUser;
            radialChart.TotalCurrent = totalAppUserCurrent;
            radialChart.hasIncreased = totalAppUserCurrent > totalAppUserPrevious;
            radialChart.Series = new int[] { ratio };

            return Json(radialChart);
        }

        public async Task<IActionResult> GetTotalIntervieweeRadial()
        {
            var interviewees = await GetUsers("Interviewee");

            int totalAppUser = interviewees.totalUser;
            int totalAppUserCurrent = interviewees.userCurrentCount;
            int totalAppUserPrevious = interviewees.userPreviousCount;

            RadialChart radialChart = new RadialChart();
            int ratio = 100;
            if (totalAppUserPrevious > 0)
            {
                ratio = Convert.ToInt32((totalAppUserCurrent - totalAppUserPrevious) / totalAppUserPrevious * 100);
            }

            radialChart.TotalCount = totalAppUser;
            radialChart.TotalCurrent = totalAppUserCurrent;
            radialChart.hasIncreased = totalAppUserCurrent > totalAppUserPrevious;
            radialChart.Series = new int[] { ratio };

            return Json(radialChart);
        }

        public async Task<IActionResult> PieChart()
        {
            var paidMeetings = await _context.Meetings.CountAsync(c => c.MeetingType == true);
            var freeMeetings = await _context.Meetings.CountAsync(c => c.MeetingType == false);

            PieChartVM pieChartVM = new PieChartVM()
            {
                Labels = new string[] { "paidMeetings", "freeMeetings" },
                Series = new decimal[] { paidMeetings, freeMeetings },
            };

            return Json(pieChartVM);
        }

        public async Task<IActionResult> GetMemberAndMeetingLineChartData()
        {
            var memberData = _context.ApplicationUsers
                .Where(u => u.CreatedAt >= DateTime.Now.AddDays(-30) && u.CreatedAt.Date <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewMemberCount = u.Count()
                })
                .ToList();

            var meetingData = _context.Meetings
                .Where(u => u.CreatedAt >= DateTime.Now.AddDays(-30) && u.CreatedAt.Date <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewMeetingCount = u.Count()
                })
                .ToList();



            var leftJoin = memberData.GroupJoin(
                meetingData,
                member => member.DateTime,
                meeting => meeting.DateTime,
                (member, meetings) => new
                {
                    DateTime = member.DateTime,
                    NewMemberCount = member.NewMemberCount,
                    NewMeetingCount = meetings.Select(m => m.NewMeetingCount).FirstOrDefault()
                });



            var rightJoin = meetingData.GroupJoin(
               memberData,
               meeting => meeting.DateTime,
               member => member.DateTime,
               (meeting, members) => new
               {
                   DateTime = meeting.DateTime,
                   NewMemberCount = members.Select(m => m.NewMemberCount).FirstOrDefault(),
                   NewMeetingCount = meeting.NewMeetingCount
               });


            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newMeetingData = mergedData.Select(x => x.NewMeetingCount).ToArray();
            var newMemberData = mergedData.Select(x => x.NewMemberCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();

            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Meetings",
                    Data = newMeetingData
                },
                new ChartData
                {
                    Name = "New Members",
                    Data = newMemberData
                },
            };

            LineChart lineChartVM = new()
            {
                Categories = categories,
                Series = chartDataList
            };



            return Json(lineChartVM);
        }

    }
}
