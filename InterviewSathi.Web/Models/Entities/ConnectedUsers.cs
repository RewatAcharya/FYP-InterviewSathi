using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class ConnectedUsers
    {
        [Key]
        public string ConnId { get; set; }

        [ForeignKey("ConnectedUser")]
        public string UserId { get; set; }

        public virtual ApplicationUser? ConnectedUser { get; set; }

    }
}
