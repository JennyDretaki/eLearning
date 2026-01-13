using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Old password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Old password must be 6-100 characters.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be 6-100 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
