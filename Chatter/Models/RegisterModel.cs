using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Models
{
    public class RegisterModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }
    }
}
