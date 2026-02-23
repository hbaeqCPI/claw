using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QERecipientView
    {
        public int RecipientID { get; set; }
        public int QESetupID { get; set; }
        public int RoleSourceID { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public string? RoleType { get; set; }
        public string? SendAs { get; set; }
        public bool IsDefault { get; set; }
        public int OrderOfEntry { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        public byte[] tStamp { get; set; }
    }
}
