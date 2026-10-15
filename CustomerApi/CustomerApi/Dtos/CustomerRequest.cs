using System.ComponentModel.DataAnnotations;

namespace CustomerApi.Dtos
{
    public class CustomerRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be in 3 to 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscore.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int  age { get; set; }

        [Required(ErrorMessage = "Contact is required")]
        [RegularExpression(@"^\+?[0-9]{8,11}$", ErrorMessage = "Contact must be 8 to 11 digits, with a optional leading +.")]
        public string Contact { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        [StringLength(50, ErrorMessage = "Email must not exceed 50 characters")]
        public string Email { get; set; } = string.Empty;
    }
}
