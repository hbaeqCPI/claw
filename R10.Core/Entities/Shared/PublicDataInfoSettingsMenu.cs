using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Shared
{ 
    public class PublicDataInfoSettingsMenu 
    {
    
        [Key]
        public int InfoMenuId { get; set; }

        [Required]
        public string?  InfoMenuCode { get; set; }

        [Required]
        public string?  MenuText { get; set; }

        [Required]
        public string?  UserControlId { get; set; }
        public bool Visible { get; set; }

       
    }

}
