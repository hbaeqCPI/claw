using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class TimeTrack : BaseEntity
    {
        [Key]
        public int TimeTrackId { get; set; }
        public string SystemType { get; set; }
        public int AttorneyID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int? AppId { get; set; }
        public int? TmkId { get; set; }
        public int? MatId { get; set; }
        public string? UserId { get; set; }
        public string? TrackUserId { get; set; }
    }
}
