namespace R10.Web.Api.Models
{
    public class AssignmentData
    {
        public int AssignmentId { get; set; }
        public string? AssignmentFrom { get; set; }
        public string? AssignmentTo { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public string? Reel { get; set; }
        public string? Frame { get; set; }
        public string? AssignmentStatus { get; set; }
        public string? Remarks { get; set; }
    }
}
