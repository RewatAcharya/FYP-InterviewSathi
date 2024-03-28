using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities
{
    public class PlatformReview : Base
    {
        [ForeignKey("User")]
        public string? SendBy { get; set; }
        public string? Message { get; set; }    
        public bool Status { get; set; } = false;
        public string? ReviewType { get; set; }

        public string? PicURL { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? PicUpload { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
