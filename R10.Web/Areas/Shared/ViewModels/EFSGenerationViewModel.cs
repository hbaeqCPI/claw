using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class EFSGenerationViewModel
    {
        public int RecId { get; set; }
        public string? SystemType { get; set; }
        public string? DocType { get; set; }
        public string? Country { get; set; }
        public string? DataKey { get; set; }
        public string? SharePointRecKey { get; set; }

        public List<EFSFormDTO>? Forms { get; set; }
        public List<LookupDTO>? Signatories { get; set; }
    }
}
