using System.ComponentModel.DataAnnotations;
using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QEDataSourceDetailViewModel : BaseEntity
    {
        public int DataSourceID { get; set; }
        [Display(Name = "Data Source Name")]
        public string? DataSourceName { get; set; }
        public string? SystemType { get; set; }

    }
}
