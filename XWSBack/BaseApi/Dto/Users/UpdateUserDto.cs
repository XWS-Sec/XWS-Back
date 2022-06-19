using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Dto.Users
{
    public class UpdateUserDto
    {
        public string Username { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string Biography { get; set; }

    }
}
