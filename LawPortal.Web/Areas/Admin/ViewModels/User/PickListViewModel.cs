using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Admin.ViewModels
{
    public class PickListViewModel
    {
        public PickListViewModel()
        {
        }

        public PickListViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        [Required]
        public string Id { get; set; }
        
        public string? Name { get; set; }
    }
}
