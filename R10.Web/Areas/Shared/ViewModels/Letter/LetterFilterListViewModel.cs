using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterFilterListViewModel
    {
        [Key]
        public int LetFilterId { get; set; }

        [Display(Name = "Data Source")]
        public string? RecSource { get; set; }

        [Display(Name = "Field Name")]
        public string? FieldName { get; set; }

        public int FieldType { get; set; }

        [Display(Name = "Condition")]
        public string? Operator { get; set; }

        [Display(Name = "Data 1")]
        public string? Operand1 { get; set; }

        [Display(Name = "Data 2")]
        public string? Operand2 { get; set; }
    }
}
