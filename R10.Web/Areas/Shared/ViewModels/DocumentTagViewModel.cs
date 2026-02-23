using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocumentTagViewModel
    {
        [Required, StringLength(50)]
        public string? Tag { get; set; }
       
    }
}
