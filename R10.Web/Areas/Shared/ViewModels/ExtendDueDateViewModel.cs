using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ExtendDueDateViewModel:DueDateExtension
    {
        
        public string? Area { get; set; }
        public string? Controller { get; set; }

        [Display(Name ="Current due date")]
        public DateTime? CurrentDueDate { get; set; }

        [Display(Name = "Extend due date to")]
        public DateTime? NewDueDateFormatted { get; set; }
        
        public DateTime? NewDueDate { get; set; }

        
    }
}
