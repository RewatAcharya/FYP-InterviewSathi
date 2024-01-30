using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.Models.Entities.BlogsEntity
{
    public class LikeCount : Base
    {
        [Required]
        [ForeignKey("User")]
        public string LikedBy { get; set; }

        [Required]
        [ForeignKey("Blog")]
        public string LikedBlog { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Blog? Blog { get; set; }
    }
}
