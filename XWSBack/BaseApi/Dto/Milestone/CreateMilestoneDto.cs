using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Dto.Milestone
{
    public class CreateMilestoneDto
    {
        [Required(ErrorMessage = "Start time is required!")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "End time is required!")]
        public DateTime EndDateTime { get; set; }

        [Required(ErrorMessage = "Title is required!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Organization name is required!")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Description is required!")]
        public string Description { get; set; }
    }
}
