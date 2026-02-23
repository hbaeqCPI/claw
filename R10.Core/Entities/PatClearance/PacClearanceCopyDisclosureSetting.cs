using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.PatClearance
{
    public class PacClearanceCopyDisclosureSetting
    {
        [Key]
        public int CopySettingId { get; set; }

        public string? FieldDesc { get; set; }
        public string? FieldName { get; set; }

        public string? DestFieldDesc { get; set; }
        public string? DestFieldName { get; set; }

        public bool Copy { get; set; }
    }
}
