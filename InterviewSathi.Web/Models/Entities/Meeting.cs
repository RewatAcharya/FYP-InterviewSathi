using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class Meeting : Base
    {
        public TimeOnly MeetingTime { get; set; }
        public DateOnly MeetingDate { get; set; }
        [ForeignKey("SendingBy")]
        public string SentBy { get; set; }
        [ForeignKey("SendingTo")]
        public string SentTo { get; set; }
        public string InterviewType { get; set; }

        [Display(Name = "Payment Type")]
        public bool MeetingType { get; set; } = false;  //where false is free
        public bool Status { get; set; } = false;
        public bool MeetingStatus { get; set; } = false;

        public virtual ApplicationUser? SendingBy { get; set; }
        public virtual ApplicationUser? SendingTo { get; set; }
    }
}
