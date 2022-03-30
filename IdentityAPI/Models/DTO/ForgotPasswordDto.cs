using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.Models.DTO
{
    public class ForgotPasswordDto
    {

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
