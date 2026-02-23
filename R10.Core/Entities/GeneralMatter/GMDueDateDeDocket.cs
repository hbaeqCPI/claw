namespace R10.Core.Entities.GeneralMatter
{

    public class GMDueDateDeDocket : DueDateDeDocket
    {
        public GMDueDate? GMDueDate { get; set; }
        public List<GMDueDateDeDocketResp>? GMDueDateDeDocketResps { get; set; }
    }

    public class GMDueDateDeDocketOutstanding : DueDateDeDocketOutStanding
    {
        public GMDueDate? GMDueDate { get; set; }
    }
}
