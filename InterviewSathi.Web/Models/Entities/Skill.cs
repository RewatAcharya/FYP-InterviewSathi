using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InterviewSathi.Web.Models.Entities
{
    public class Skill : Base
    {
        [Required]
        [DisplayName("Skill")]
        public string NameOfSkill { get; set; }

        [DisplayName("Description")]
        public string DescofSkill { get; set; }
    }

}
