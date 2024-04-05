using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities.BlogsEntity
{
    public class Blog : Base
    {
        public string Content { get; set; }
        public string? ImgPath { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        [DisplayName("Image")]
        public IFormFile? BlogPath { get; set; }
        public int LikeCount { get; set; }

        public bool IsDeleted { get; set; } = false;

        [ForeignKey("User")]
        public string PostedBy { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
