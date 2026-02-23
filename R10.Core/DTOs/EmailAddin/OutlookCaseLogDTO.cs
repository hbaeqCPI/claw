using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    [Keyless]
    public class OutlookCaseLogDTO
    {
        public string? SystemName { get; set; }
        public string? Screen { get; set; }
        public string? CaseInfo { get; set; }
        //public DateTime DateCreated { get; set; }
        public string? DateCreated { get; set; }

    }
}
