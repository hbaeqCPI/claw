using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities
{
    public class DataQueryAllowedFunction : BaseEntity
    {
        [Key]
        public int FnId { get; set; }
        [Display(Name = "Function")]
        public string? FunctionName { get; set; }
        [Display(Name = "Description")]
        public string? FunctionDescription { get; set; }
    }
}
