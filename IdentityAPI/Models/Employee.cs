using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int Age { get; set; }
        [Required]
        public string Position { get; set; } = string.Empty;
    }
}
