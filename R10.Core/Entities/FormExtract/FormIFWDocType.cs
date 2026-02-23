using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.FormExtract
{
    public class FormIFWDocType
    {
        [Key]
        public int DocTypeId { get; set; }
        
        public int FormTypeId { get; set; }
        public string DocDesc { get; set; }
        public string ModelId { get; set; }
        public string ScanPages { get; set; }

        public bool IsEnabled { get; set; }
        public string SystemType { get; set; }

        public FormIFWFormType FormIFWFormType { get; set; }
        public List<FormIFWActMap> FormIFWActMaps { get; set; }
        public List<RTSSearchUSIFW> RTSSearchUSIFWs { get; set; }
        public List<TLSearchDocument> TLSearchDocuments { get; set; }
    }
}
