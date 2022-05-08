using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Dto.Milestone
{
    public class DeleteMilestoneDto
    {
        [Required(ErrorMessage = "Title is required!")]
        public string Title { get; set; }

        [Required(ErrorMessage ="Start time is required!")]
        public DateTime StartTime { get; set; }

    }
}
