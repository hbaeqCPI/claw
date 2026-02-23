
namespace R10.Core.Entities.Patent
{
    public class PatDueDateDeDocket : DueDateDeDocket
    {
        public PatDueDate? PatDueDate { get; set; }
        public List<PatDueDateDeDocketResp>? PatDueDateDeDocketResps { get; set; }
    }

    public class PatDueDateDeDocketOutstanding : DueDateDeDocketOutStanding
    {
        public PatDueDate? PatDueDate { get; set; }

    }
}
