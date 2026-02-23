using System.ComponentModel.DataAnnotations;
using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterDataSourceDetailViewModel : BaseEntity
    {
        public int DataSourceId { get; set; }
        [Display(Name = "Data Source Name")]
        public string? DataSourceDescMain { get; set; }
        public string? DataSourceDescDtl { get; set; }
        public string? SystemType { get; set; }

    }
}
