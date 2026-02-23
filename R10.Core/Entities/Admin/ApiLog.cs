using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities
{
    public class ApiLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Request Method")]
        public string? RequestMethod { get; set; }

        [Required]
        [Display(Name = "Request Url")]
        public string? RequestUrl { get; set; }

        [Required]
        [Display(Name = "Status Code")]
        public int StatusCode { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "User Id")]
        public string? UserId { get; set; }

        [Required]
        [Display(Name = "Time Stamp")]
        public DateTime TimeStamp { get; set; }

        [Required]
        [Display(Name = "Allowance")]
        public int? Allowance { get; set; }

        [Required]
        [Display(Name = "Allowance Used")]
        public int? AllowanceUsed { get; set; }

        [Required]
        [Display(Name = "Cost")]
        public int? Cost { get; set; }
    }
}
