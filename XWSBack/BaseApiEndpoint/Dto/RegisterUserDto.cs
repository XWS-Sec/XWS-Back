using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BaseApi.Dto
{
    public class RegisterUserDto : LoginUserDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Surname is required")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "You need to repeat the password")]
        [Compare(nameof(RegisterUserDto.Password), ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }

        public IFormFile Picture { get; set; }
    }
}