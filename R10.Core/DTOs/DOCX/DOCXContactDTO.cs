using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    //[Keyless]
    //public class DOCXContactDTO : DOCXEntityContactDTO
    //{
    //    [Display(Name = "Prev Gen?")]
    //    public bool IsPrevGen { get; set; }

    //    [Display(Name = "Entity Code")]
    //    public string? EntityCode { get; set; }

    //    [Display(Name = "Entity Name")]
    //    public string? EntityName { get; set; }

    //    [Display(Name = "Contact Name")]
    //    public string? ContactName { get; set; }

    //    [Display(Name = "Contact Address")]
    //    public string? ContactAddress { get; set; }

    //}

    public class DOCXEntityContactDTO
    {
        public int EntityId { get; set; }
        public int ContactId { get; set; }

    }
}
