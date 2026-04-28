using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class DocViewDTO
    {
        public string? DocFileName { get; set; }
        public string? DocUrl { get; set; }
        //public string? DocFolder { get; set; }
    }
}
