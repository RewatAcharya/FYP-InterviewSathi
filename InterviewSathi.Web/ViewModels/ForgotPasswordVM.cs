using System.ComponentModel.DataAnnotations;

namespace InterviewSathi.Web.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
