using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class PTOMappingPageViewModel
    {
        public string MappingType { get; set; }
        public string PageId { get; set; }
        public string SystemType { get; set; }
        public int GridPageSize { get; set; } = 20;

        public DetailPagePermission PagePermission { get; set; }

        public string ToolbarTemplate(string addUrl, string addLabel, string exportToExcelLabel, string exportSetupLabel = "")
        {
            var excelButton = $"<a class='k-button k-button-icontext k-grid-excel no-results-hide' href='\\#'><i class='fal fa-file-excel pr-2'></i>{exportToExcelLabel}</a>";
            var addButton = !string.IsNullOrEmpty(addUrl) ? $"<a class='k-button page-nav' data-url='{addUrl}' href='\\#'><i class='fal fa-plus pr-2'></i>{addLabel}</a>" : "";
            return $"<nav class='nav nav-left sidebar-link'>{excelButton}{addButton}</nav>";
        }

    }

    
}
