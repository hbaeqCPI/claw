
namespace R10.Core.Entities.Trademark
{

    public class TmkDueDateDeDocket : DueDateDeDocket
    {
        public TmkDueDate? TmkDueDate { get; set; }
        public List<TmkDueDateDeDocketResp>? TmkDueDateDeDocketResps { get; set; }
    }

    public class TmkDueDateDeDocketOutstanding : DueDateDeDocketOutStanding
    {
        public TmkDueDate? TmkDueDate { get; set; }
    }
}
