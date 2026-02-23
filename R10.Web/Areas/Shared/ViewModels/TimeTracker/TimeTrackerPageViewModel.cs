namespace R10.Web.Areas.Shared.ViewModels
{
    public class TimeTrackerPageViewModel
    {
        public int Id { get; set; }
        public string SystemType { get; set; }
        public List<TimeTrackAttorney> Attorneys { get; set; }
        public string? AttorneyIds { get; set; }
    }
}
