using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Clearance
{
    public class TmcClearanceCopySetting
    {
        [Key]
        public int CopySettingId { get; set; }
        public string FieldDesc { get; set; }
        public string FieldName { get; set; }
        public bool Copy { get; set; }
    }
}
