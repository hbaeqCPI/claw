namespace R10.Web.Api.Models
{
    public class ActionDueData
    {
        public int ActId { get; set; }
        public string CaseNumber { get; set; }
        public string Country { get; set; }
        public string? SubCase { get; set; }
        public string? ActionType { get; set; }
        public DateTime BaseDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public bool ComputerGenerated { get; set; }

        public string? AttorneyCode { get; set; }
        public string? AttorneyName { get; set; }

        public string? Remarks { get; set; }

        public List<DueDateData>? DueDates { get; set; }
    }
}
