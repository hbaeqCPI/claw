using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEPOMailLog : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }
        public int LogId { get; set; }
        public int AppId { get; set; }
        public string? SearchStr { get; set; }
        public string? SearchField { get; set; }
        public string? CommunicationId { get; set; }

        public EPOCommunication? EPOCommunication { get; set; }
    }    
}
