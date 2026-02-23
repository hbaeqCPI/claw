using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Identity
{
    public class CPiUserTypeDefaultWidget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public CPiUserType UserType { get; set; }

        [Required]
        public int WidgetId { get; set; }

        [Required]
        public int WidgetCategory { get; set; }

        public int SortOrder { get; set; }

        public CPiWidget? CPiWidget { get; set; }
    }
}
