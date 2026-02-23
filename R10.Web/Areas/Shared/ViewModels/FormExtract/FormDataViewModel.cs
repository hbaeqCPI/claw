using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class FormDataViewModel
    {
        [Display(Name = "Field")]
        public string? FieldName { get; set; }

        [Display(Name = "Field Label")]
        public string? FieldLabel { get; set; }

        [Display(Name = "Data")]
        public string? FieldData { get; set; }

        [Display(Name = "Confidence")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double Confidence { get; set; }

    }
}
