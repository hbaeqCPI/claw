using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core.Entities.Documents
{
    public class DocMatterTree
    {
        [Key]
        public int NodeId { get; set; }

        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public string? ScreenName { get; set; }
        public string? SearchTabView { get; set; }
        public string? SearchResultView { get; set; }

        public bool InUse { get; set; }

        public int EntryOrder { get; set; }

    }
}
