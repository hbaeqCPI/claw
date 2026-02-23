using R10.Core.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.GlobalSearch
{
    public class GSSystem
    {
        [Key]
        public string SystemType { get; set; }
        public string SystemName { get; set; }
        public bool IsEnabled { get; set; }
        public int EntryOrder { get; set; }

        public CPiSystem CPiSystem { get; set; }
        public List<GSScreen> GSScreens { get; set; }

    }
}
