using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{

    public class RegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must contain uppercase, lowercase, digit, and special char")]
        public string Password { get; set; }

        [Required]
        [RegularExpression("^(Student|Lecturer)$", ErrorMessage = "Invalid role")]
        public string Role { get; set; }
    }
}