using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.DTOs
{
    public class DocumentVerificationSearchCriteriaDTO
    {
        // NOTE: Order of the fields must match stored procedure
        //Shared filters
        public string? SystemTypes { get; set; }
        public string? DocName { get; set; }
        public string? DocUploadedBy { get; set; }
        public DateTime? FromUploadedDate { get; set; }
        public DateTime? ToUploadedDate { get; set; }        

        public string? Client { get; set; }
        public string? Country { get; set; }
        public string? Source { get; set; }
        
        public string? RespDocketing { get; set; }
        public bool? HasRespDocketing { get; set; }
        
        public string? RespReporting { get; set; }
        public bool? HasRespReporting { get; set; }
        public bool? DocSentToClient { get; set; }

        public string? ActionType { get; set; }        

        //Docketing Requests tab
        public bool? IncludeDocumentRequest { get; set; }
        public bool? IncludeRequestDocket { get; set; }
        public bool? RequestDocketCompleted { get; set; }
        public bool? IncludeDeDocket { get; set; }
        public bool? DeDocketCompleted { get; set; }


        //Action tab filters        
        public bool? DocCheckAct { get; set; }
        public bool? ActVerified { get; set; }
        public bool? ActCheckDocket { get; set; }
        public string? ActCreatedBy { get; set; }
        public DateTime? FromActDateCreated { get; set; }
        public DateTime? ToActDateCreated { get; set; }
        public DateTime? FromDueDate { get; set; }
        public DateTime? ToDueDate { get; set; }
        public string? Attorney { get; set; }
        public string? FilterAtty { get; set; }        


        public string? SortCol { get; set; }

        public string? SortOrder { get; set; }

        public int? MaximumRows { get; set; }

        public int? StartRowIndex { get; set; }

        public string? TargetData { get; set; } = "docList";

        public string? UserName { get; set; }
    }
}
