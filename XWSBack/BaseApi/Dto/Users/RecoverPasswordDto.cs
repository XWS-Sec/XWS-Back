using System.ComponentModel.DataAnnotations;

namespace BaseApi.Dto.Users
{
    public class RecoverPasswordDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage = "You need to repeat the password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match")]
        public string NewConfirmPassword { get; set; }
    }
}