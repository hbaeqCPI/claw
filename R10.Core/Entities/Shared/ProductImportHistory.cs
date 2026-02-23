using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    
    public class ProductImportHistory
    {
        [Key]
        public int ImportId { get; set; }

        [Display(Name ="Date")]
        public DateTime ImportDate { get; set; }

        [Display(Name = "Filename")]
        public string?  OrigFileName { get; set; }
        public string?  FileName { get; set; }
        

        [Display(Name = "Records")]
        public int NoOfRecords { get; set; }

        [Display(Name = "Product Imported")]
        public int NoOfRecordsImported { get; set; }

        [Display(Name = "Product Updated")]
        public int NoOfRecordsUpdated { get; set; }

        [Display(Name = "Sale Imported")]
        public int NoOfSalesImported { get; set; }

        [Display(Name = "Sale Update")]
        public int NoOfSalesUpdated { get; set; }

        public string?  Status { get; set; }
        public string?  Options { get; set; }

        [Display(Name = "Imported By")]
        public string?  ImportedBy { get; set; } 


        [Display(Name = "Status")]
        [NotMapped]
        public string?  TranslatedStatus { get; set; }

    }
}
