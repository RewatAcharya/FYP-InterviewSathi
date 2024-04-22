using System.ComponentModel.DataAnnotations;

namespace InterviewSathi.Web.ViewModels
{
    public class ResetPasswordVM
    {
        public string UserId { get; set; }

        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        [ComplexPassword(ErrorMessage = "Invalid password.")]
        public string ConfirmPassword { get; set; }


    }
}
