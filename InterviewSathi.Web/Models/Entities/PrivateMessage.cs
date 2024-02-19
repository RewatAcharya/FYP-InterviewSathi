using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class PrivateMessage : Base
    {
        public string MessageContent { get; set; }
        [ForeignKey("SendingBy")]
        public string SenderId { get; set; }
        [ForeignKey("SendingTo")]
        public string ReceiverId { get; set; }
        public bool ReadStatus { get; set; } = false;

        public virtual ApplicationUser? SendingBy { get; set; }
        public virtual ApplicationUser? SendingTo { get; set; }
    }
}
