using System.ComponentModel.DataAnnotations;

namespace BaseApi.Dto.Company
{
    public class NewCompanyDto
    {
        [Required(ErrorMessage = "Link to company is mandatory")]
        [Url(ErrorMessage = "Not a valid url to company")]
        public string LinkToCompany { get; set; }
        [Required(ErrorMessage = "Name of company is mandatory")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email of company is mandatory")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "PhoneNumber of company is mandatory")]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}