using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class ReviewRating : Base
    {
        public int Star { get; set; }
        public string Review { get; set; }
        [Required]
        [ForeignKey("User")]
        public string RatedBy { get; set; }

        [Required]
        [ForeignKey("User1")]
        public string RatedTo { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual ApplicationUser? User1 { get; set; }
    }
}
