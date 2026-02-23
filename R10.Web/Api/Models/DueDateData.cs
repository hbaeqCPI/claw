namespace R10.Web.Api.Models
{
    public class DueDateData
    {
        public int DDId { get; set; }
        public string? ActionDue { get; set; }
        public DateTime DueDate { get; set; }
        public string? Indicator { get; set; }
        public DateTime? DateTaken { get; set; }

        public string? AttorneyCode { get; set; }
        public string? AttorneyName { get; set; }

        public string? Remarks { get; set; }
    }
}
