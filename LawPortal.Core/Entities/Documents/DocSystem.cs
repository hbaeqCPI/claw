using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core.Entities.Documents
{
    public class DocSystem
    {
        [Key]
        public string? SystemType { get; set; }
        public string? SystemName { get; set; }
        public string? SystemNameShort { get; set; }
        public bool IsEnabled { get; set; }
        public int EntryOrder { get; set; }

        public CPiSystem? CPiSystem { get; set; }
    }
}
