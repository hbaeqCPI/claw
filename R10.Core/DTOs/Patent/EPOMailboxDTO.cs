using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    #region MyEPO
    public class EPODocumentCode
    {
        public string? DocCode { get; set; }
        public string? EnglishDesc { get; set; }
        public string? FrenchDesc { get; set; }
        public string? GermanDesc { get; set; }
    }

    public class EPOMailboxInput
    {
        [Required]
        public string? access_token { get; set; }

        [Required]
        public string? SearchField { get; set; } = "userReference";

        public int DiffTolerance { get; set; } = 1;

        public DateTime? DispatchDateFrom { get; set; }
                
        public List<EPOMailboxSearchInput>? SearchInputs { get; set; }
    }

    public class EPOMailboxSearchInput
    {
        public int AppId { get; set; }
        public string? SearchStr { get; set; }        
        public List<string>? ExistingCommIds { get; set; }
    }

    public class EPOMailboxCommunicationInput
    {
        [Required]
        public string? access_token { get; set; }
        [Required]
        public List<string>? CommunicationIds { get; set; }
    }

    public class EPOmailboxHandleOutput
    {
        public string? CommunicationId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EPOMailboxOutput
    {
        public int AppId { get; set; }        
        public List<EPOMailboxCommunication>? Communications { get; set; }
    }

    public class EPOMailboxCommunication
    {        
        public string? id { get; set; }

        public string? applicantName { get; set; }

        public string? title { get; set; }

        public DateTime? dispatchDate { get; set; }

        public string? applicationNumber { get; set; }

        public string? recipientName { get; set; }

        public string? userReference { get; set; }

        public string? document { get; set; }

        public string? digitalFile { get; set; }

        public string? documentCode { get; set; }

        public bool read { get; set; }

        public bool handled { get; set; }
 
        public string? folderId { get; set; }

        //public List<M2MLink>? links { get; set; }

        public List<DocumentByte>? Documents { get; set; }
    }
    
    public class EPOPortfolioOutput
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? type { get; set; }
        public bool? hasFullAccess { get; set; }
    }

    public class EPOApplicationOutput
    {
        public string? appProcedure { get; set; }
        public string? ipOfficeCode { get; set; }
        public string? applicationNumber { get; set; }
        public string? applicationNumberMyEpo { get; set; }
        public DateTime? filingDate { get; set; }
        public string? applicantFileReference { get; set; }
        public string? portfolioId { get; set; }
        public string? portfolioName { get; set; }
        public string? procedure { get; set; }        
    }

    public class EPODueDateOutput
    {
        public string? procedure { get; set; }
        public string? ipOfficeCode { get; set; }
        public string? applicationNumber { get; set; }
        public string? applicationNumberMyEpo { get; set; }
        public DateTime? filingDate { get; set; }
        public string? termKey { get; set; }        
        public DateTime? dueDate { get; set; }
        public string? actor { get; set; }
    }

    public class EPODueDateTermOutput
    {        
        public string? termKey { get; set; }        
        public string? descriptionEN { get; set; }        
        public string? descriptionFR { get; set; }        
        public string? descriptionDE { get; set; }
    }
    #endregion

    #region OPS
    public class EPOOPSFirstImageInput
    {
        [Required]
        public string? access_token { get; set; }        
                
        public List<EPOOPSFirstImageSearchInput>? SearchInputs { get; set; }
    }

    public class EPOOPSFirstImageSearchInput
    {
        public int AppId { get; set; }
        public string? Number { get; set; }
    }

    public class EPOOPSFirstImageOutput : DocumentByte
    {
        public int AppId { get; set; }
        public string? Number { get; set; }
    }
    #endregion

    public class DocumentByte
    {
        public string? FileName { get; set; }
        public byte[]? Data { get; set; }
    }

}
