using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class ListDTO
    {
        public string? Id { get; set; }
        public string? System { get; set; }
        public string? Title { get; set; }
        public string? CaseNumber { get; set; }
    }
}
