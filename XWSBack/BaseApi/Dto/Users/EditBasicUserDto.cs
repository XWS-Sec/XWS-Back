﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Dto.Users
{
    public class EditBasicUserDto
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage ="Privacy option is required")]
        public bool IsPrivate { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string Biography { get; set; }

    }
}
