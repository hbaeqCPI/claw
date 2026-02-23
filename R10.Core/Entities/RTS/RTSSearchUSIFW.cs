using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.FormExtract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSSearchUSIFW
    {
        public int PLAppID { get; set; }
        public int OrderOfEntry { get; set; }
        public DateTime MailRoomDate { get; set; }
        public string? Description { get; set; }
        public int NoPages { get; set; }
        public int PageStart { get; set; }
        public string? FileName { get; set; }
        public string? DocumentCode { get; set; }
        public bool? HasDocument { get; set; }
        public bool? Transferred { get; set; }

        [Key]
        public int IFWId { get; set; }
        public int DocTypeId { get; set; }

        public DateTime? AIParseDate { get; set; }
        public DateTime? AIActionGenDate { get; set; }
        public DateTime? AIRemUpdDate { get; set; }
        public bool? AIActionGenError { get; set; }
        public bool AIInclude { get; set; }


        public string? DocName { get; set; }

        public bool IsActRequired { get; set; }
        public bool CheckAct { get; set; }
        public bool SendToClient { get; set; }

        
        public RTSSearch? RTSSearch { get; set; }
        public FormIFWDocType? FormIFWDocType { get; set; }
    }  
}
