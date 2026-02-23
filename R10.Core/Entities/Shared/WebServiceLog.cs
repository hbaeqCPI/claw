using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class WebServiceLog
    {      
        [Key]
        public int LogId { get; set; }
        public string? LoginName { get; set; }
        public string? RequestPath { get; set; }    //request path and query
        public DateTime? RunDate { get; set; }      //execution start timestamp
        public DateTime? EndDate { get; set; }      //execution end timestamp
        public int RecordCount { get; set; }        //records affected
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public string? Method { get; set; }
    }
}