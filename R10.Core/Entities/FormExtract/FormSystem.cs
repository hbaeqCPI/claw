using R10.Core.Identity;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.FormExtract
{
    public class FormSystem
    {
        [Key]
        public string SystemType { get; set; }
        public string SystemName { get; set; }
        public bool IsEnabled { get; set; }
        public int EntryOrder { get; set; }

        public CPiSystem CPiSystem { get; set; }
    }
}
