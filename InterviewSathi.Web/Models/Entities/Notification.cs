using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class Notification : Base
    {
        [ForeignKey("SendingBy")]
        public string SentBy { get; set; }
        [ForeignKey("SendingTo")]
        public string SentTo { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; } = false;

        public virtual ApplicationUser? SendingBy { get; set; }
        public virtual ApplicationUser? SendingTo { get; set; }
    }
}
