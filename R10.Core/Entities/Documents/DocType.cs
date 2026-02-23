using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Documents
{
    public class DocType : BaseEntity
    {
        [Key]
        public int DocTypeId { get; set; }
        
        public string? DocTypeName { get; set; }
        public string? DocTypeImage { get; set; }
        public string? RegExFilter { get; set; }

        public int EvalOrder { get; set; }

        public List<TmkImage>? TmkImages { get; set; }
        public List<PatAppImage>? PatAppImages { get; set; }
        public List<DMSFaqDoc>? DMSFaqDocs { get; set; }
        public List<InventionImage>? InventionImages { get; set; }
    }
}
