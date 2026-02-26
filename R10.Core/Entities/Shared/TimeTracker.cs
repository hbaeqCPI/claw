// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class TimeTracker:TimeTrackerDetail
    {
        public Attorney? Attorney { get; set; }
        public CountryApplication? CountryApplication { get; set; }
        public TmkTrademark? TmkTrademark { get; set; }
//         public GMMatter? GeneralMatter { get; set; } // Removed during deep clean
    }

    public class TimeTrackerDetail : BaseEntity
    {
        [Key]
        public int TimeTrackerId { get; set; }
        public int AttorneyID { get; set; }
        public int? AppId { get; set; }
        public int? TmkId { get; set; }
        public int? MatId { get; set; }
        public int? CostTrackId { get; set; }
        public string SystemType { get; set; }
        [Range(0.01, 10000.00)]
        public decimal Duration { get; set; }
        public DateTime EntryDate { get; set; }
        public string? Description { get; set; }
        
        public bool Exported { get; set; }
        public DateTime? ExportedDate { get; set; }
        public string? TrackUserId { get; set; }

    }
}
