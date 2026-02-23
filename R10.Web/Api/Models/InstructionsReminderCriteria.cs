namespace R10.Web.Api.Models
{
    public class InstructionsReminderCriteria
    {
        public List<string>? AnnuityCodes { get; set; }
        public int NumberOfDaysDue { get; set; }
        public bool AlertOnly { get; set; }
    }
}
