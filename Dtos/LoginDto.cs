using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{

    public class LoginDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }
    }
}