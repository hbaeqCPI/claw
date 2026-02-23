using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Shared
{

    public class PublicDataInfoSettingsMenuCountry
    {

        [Key]
        public int Id { get; set; }

        public int InfoMenuId { get; set; }

        [Required]
        public string?  InfoMenuCode { get; set; }

        [Required]
        public string?  Country { get; set; }
        public int Sequence { get; set; }
        public bool Active { get; set; }

       
    }
    

}
