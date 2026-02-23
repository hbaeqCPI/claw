using R10.Core.Entities.FormExtract;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace R10.Core.Entities.Trademark
{
    public class TLSearchDocument
    {
        [Key]
        public int TLDocId { get; set; }
        public int TLTmkId { get; set; }
        public DateTime MailDate { get; set; }
        public string? Description { get; set; }
        public string? Target { get; set; }
        public string? FileName { get; set; }
        public bool? Transferred { get; set; }

        public TLSearch? TLSearch { get; set; }
        public string? DocName { get; set; }

        public int DocTypeId { get; set; }
        public DateTime? AIParseDate { get; set; }
        public DateTime? AIActionGenDate { get; set; }

        public FormIFWDocType? FormIFWDocType { get; set; }
    }


}
