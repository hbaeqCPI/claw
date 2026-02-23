using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterDataSourceViewModel
    {
        public int DataSourceId { get; set; }

        [Display(Name = "Available Data Source")]
        public string? DataSourceDescMain { get; set; }

    }
}
