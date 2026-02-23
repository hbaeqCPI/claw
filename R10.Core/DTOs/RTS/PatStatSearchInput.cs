using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    public class PatStatSearchCitationInput
    {        
        public int AppId { get; set; }
        public string? PatNumber { get; set; }
        public string? Country { get; set; }

    }
}
