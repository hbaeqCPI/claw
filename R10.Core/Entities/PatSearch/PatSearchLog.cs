using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatSearchNotifyLog
    {
        [Key]
        public int SearchId { get; set; }
        public int CritDtlId { get; set; }
        public string? CriteriaName { get; set; }
        public DateTime SearchDate { get; set; }
        public string? SearchMode { get; set; }
        public string? Criteria { get; set; }
        public string? EmailsToNotify { get; set; }
        public string? Result { get; set; }
        public string? NewEntries { get; set; }
    }
}




    
	
	
	
	
	