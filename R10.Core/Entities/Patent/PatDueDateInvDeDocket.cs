
namespace R10.Core.Entities.Patent
{
    public class PatDueDateInvDeDocket : DueDateDeDocket
    {
        public PatDueDateInv? PatDueDateInv { get; set; }
    }

    public class PatDueDateInvDeDocketOutstanding : DueDateDeDocketOutStanding
    {
        public PatDueDateInv? PatDueDateInv { get; set; }

    }

}
