using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewSathi.Web.ViewModels
{
    public class ExternalLoginConfVM
    {
        [Required]
        public string Name { get; set; }
        [Required(ErrorMessage = "Role must be selected before proceeding")]
        public string Role { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? DocUpload { get; set; }
    }
}
