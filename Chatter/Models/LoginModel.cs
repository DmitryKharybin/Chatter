using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public required string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
