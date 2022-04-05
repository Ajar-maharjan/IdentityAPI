using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.Models.DTO
{
    public class AuthenticateRequestDto
    {

        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}

