using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities.BlogsEntity
{
    public class Comment : Base
    {
        [Required]
        [ForeignKey("User")]
        public string CommentBy { get; set; }

        [Required]
        [ForeignKey("Blog")]
        public string CommentBlog { get; set; }
        public string Content { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Blog? Blog { get; set; }
    }
}
