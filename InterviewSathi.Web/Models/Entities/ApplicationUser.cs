using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InterviewSathi.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string? ProfileURL { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ProfileUpload { get; set; }

        public string? CoverURL { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? CoverUpload { get; set; }

        public string? DocURL { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? DocUpload { get; set; }

        public string? Bio { get; set; }
        public bool? IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
