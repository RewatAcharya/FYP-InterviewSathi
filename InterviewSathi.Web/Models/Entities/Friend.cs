using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class Friend : Base
    {
        [ForeignKey("SendingBy")]
        public string SentBy { get; set; }
        [ForeignKey("SendingTo")]
        public string SentTo { get; set; }
        public bool Accepted { get; set; } = false;

        public virtual ApplicationUser? SendingBy { get; set; }
        public virtual ApplicationUser? SendingTo { get; set; }
    }
}
