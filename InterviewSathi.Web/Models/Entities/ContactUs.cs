using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class ContactUs : Base
    {
        public string? SenderName { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
        public bool? IsViewed { get; set; } = false;

        [ForeignKey("User")]
        public string? Sender { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
