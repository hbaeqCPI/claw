using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatInventorInvWebSvc : PatInventorWebSvc
    {
        [Key]
        public int EntityId { get; set; }

        public int InvEntityId { get; set; }

        public InventionWebSvc? Invention {  get; set; }
    }

    public class PatInventorWebSvc
    {
        [Display(Name = "EMail")]
        [EmailAddress(ErrorMessage = "The Email address is not valid.")]
        [StringLength(150)]
        [Required]
        public string? EMail { get; set; }

        [StringLength(25)]
        [Display(Name = "Last Name")]
        [Required]
        public string? LastName { get; set; }

        [StringLength(25)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [StringLength(25)]
        [Display(Name = "Middle Name")]
        public string? MiddleInitial { get; set; }
    }
}
