using InterviewSathi.Web.Models.Entities.BlogsEntity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class UserSkill : Base
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [Required]
        [ForeignKey("Skill")]
        public string SkillId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Skill? Skill { get; set; }
    }
}
