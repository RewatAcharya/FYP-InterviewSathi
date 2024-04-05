using InterviewSathi.Web.Models.Entities;

namespace InterviewSathi.Web.ViewModels
{
    public class ExpertVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? Profile { get;  set; }
        public string? DocURL { get;  set; }
        public bool? IsVerified { get;  set; }
        public List<UserSkill> Skills { get; set; }
        public string? Email { get;  set; }
        public DateTime? CreatedAt { get;  set; }
    }
}
