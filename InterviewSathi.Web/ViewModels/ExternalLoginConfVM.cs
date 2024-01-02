using System.ComponentModel.DataAnnotations;

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
    }
}
