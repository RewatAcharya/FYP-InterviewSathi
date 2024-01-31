using InterviewSathi.Web.Models.Entities;

namespace InterviewSathi.Web.ViewModels
{
    public class ExpertVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? Profile { get; internal set; }
        public List<UserSkill> Skills { get; internal set; }
    }
}
