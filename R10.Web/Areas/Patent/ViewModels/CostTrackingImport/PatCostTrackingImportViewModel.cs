using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostTrackingImportViewModel
    {
        public int ImportId { get; set; }             
        public bool FileModified { get; set; }

    }

    public class PatCostTrackingImportTypeViewModel
    {
        [Display(Name ="Column")]
        public string? ColumnName { get; set; }

        [Display(Name = "Required?")]
        public bool Required { get; set; }

        [Display(Name = "Type")]
        public string? DataType { get; set; }

        [Display(Name = "Max Length")]
        public string? MaxLength { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
        
    public class PatCostTrackingImportOptionsViewModel
    {
        public bool IgnoreDupes { get; set; }
    }
    
}
