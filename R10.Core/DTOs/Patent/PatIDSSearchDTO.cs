using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    [Keyless]
    public class PatIDSSearchInputDTO
    {
        public int RelatedCasesId { get; set; }
        public int AppId { get; set; }        
        public string? SearchStr { get; set; }    
        public string? KindCode { get; set; }
    }

    public class PatIDSSearchOutDTO
    {
        public int RelatedCasesId { get; set; }
        public int AppId { get; set; }
        public string? SearchStr { get; set; }
        public string? KindCode { get; set; }
        public string? FileUrl { get; set; }
        public string? AzureFilePath { get; set; }
    }

    public class PatIDSSearchApi
    {
        public List<int>? AppIds { get; set; }
        public int TimeOut { get; set; } = 0;
        public int MaxAttempts { get;set; } = 5;
        public int DiffTolerance { get; set; } = 1;
        public int MaxToSearch { get; set; } = 25;      //maximum number of record to search and download each api call
    }

    public class PatIDSSearchParam
    {
        public List<PatIDSSearchInputDTO> Criteria { get; set; }     
        public int DiffTolerance { get; set; }
    }
}
