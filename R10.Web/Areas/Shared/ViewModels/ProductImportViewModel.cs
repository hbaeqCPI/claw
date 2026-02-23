using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductImportViewModel
    {
        public int ImportId { get; set; }             
        public bool FileModified { get; set; }

    }

    public class ProductImportTypeViewModel
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
        
    public class ProductImportOptionsViewModel
    {
        public bool IgnoreDupes { get; set; }
        public bool UpdateProducts { get; set; }
        public bool UpdateSales { get; set; }
    }
    
}
