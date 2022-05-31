using System.ComponentModel.DataAnnotations;

namespace BaseApi.Dto.JobOffer
{
    public class NewJobOfferDto
    {
        [Required(ErrorMessage = "Link to offer is mandatory")]
        [Url(ErrorMessage = "Not a valid url")]
        public string LinkToJobOffer { get; set; }
        [Required(ErrorMessage = "Description is mandatory")]
        public string Description { get; set; }
        [Required(ErrorMessage = "JobTitle is mandatory")]
        public string JobTitle { get; set; }
        [Required(ErrorMessage = "Prerequisites are mandatory")]
        public string Prerequisites { get; set; }
    }
}