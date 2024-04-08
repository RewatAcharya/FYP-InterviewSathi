using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace InterviewSathi.Web.ViewModels
{
    public class RegisterVM
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm password")]
        [ComplexPassword(ErrorMessage = "Invalid password.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? DocUpload { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? RedirectUrl { get; set; }
        public string Role { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? RoleList { get; set; }
    }

    public class ComplexPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || !(value is string))
            {
                return false;
            }

            string password = (string)value;

            // Define individual regex patterns and error messages
            var patterns = new[]
            {
                (Pattern: @"[a-z]", ErrorMessage: "Password must have at least one lowercase letter."),
                (Pattern: @"[A-Z]", ErrorMessage: "Password must have at least one uppercase letter."),
                (Pattern: @"\d", ErrorMessage: "Password must have at least one digit."),
                (Pattern: @"[@$!%*?&]", ErrorMessage: "Password must have at least one special character.")
            };

            foreach (var (pattern, errorMessage) in patterns)
            {
                if (!Regex.IsMatch(password, pattern))
                {
                    ErrorMessage = errorMessage;
                    return false;
                }
            }

            // Additional length check
            if (password.Length < 8 || password.Length > 15)
            {
                ErrorMessage = "Password must be between 8 and 15 characters long.";
                return false;
            }

            return true;
        }
    }
}
