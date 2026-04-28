using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace LawPortal.Web.Models.PageViewModels
{
    public class PageViewModel
    {
        public PageType Page { get; set; }
        public string PageId { get; set; }
        public string Title { get; set; }
        public bool CanAddRecord { get; set; }
        public int RecordId { get; set; }
        public bool SingleRecord { get; set; }
        public bool FromSearch { get; set; }
        public string ActiveTab { get; set; }
        public int GridPageSize { get; set; } = 20;
        public DetailPagePermission PagePermission { get; set; }
        public bool HasSavedCriteria { get; set; }
        public string SystemType { get; set; }

        //testing add new record using multistep
        public string PartialViewName { get; set; }

        public string BeforeSubmit { get; set; }
        public string AfterInsert { get; set; }
        public object AfterInsertOptions { get; set; }
        public string AfterCancelledInsert { get; set; }

        public object Data { get; set; }

        public string DetailsLinkTemplate(string url, string columnName)
        {
            return $"<a href='{url}' class='details-link'>#: {columnName}#</a>";
        }

        // dont delete the function below, we might needit again in the future
        //public string DetailsLinkTemplateWithDefaultImage(string url, string columnName, string imageFileName)
        //{
        //    return $"<a href='{url}' class='details-link'><span onmouseover=\"showDefaultImage(\'{imageFileName}\')\" onmouseout=\"hideDefaultImage(\'{imageFileName}\')\">#: {columnName}#</span></a>";
        //}

        public string ExportToExcelButton(string exportToExcelLabel, bool hasExportToExcelSetup = false,string exportSetupLabel="")
        {
            var setupButton = String.Empty;
            if (hasExportToExcelSetup) {
                setupButton = $"<a class='k-button k-button-icontext k-export-settings' title='{exportSetupLabel}' href='\\#'><i class='fal fa-cog pr-2'></i>{exportSetupLabel}</a>";
            }
            var excelButton = $"<a class='k-button k-button-icontext k-grid-excel no-results-hide' href='\\#'><i class='fal fa-file-excel pr-2'></i>{exportToExcelLabel}</a>";
            return $"{excelButton}{setupButton}";
        }

        public string AddButton(string addUrl, string addLabel)
        {
            return this.CanAddRecord && !string.IsNullOrEmpty(addUrl) ? $"<a class='k-button page-nav' data-url='{addUrl}' href='\\#'><i class='fal fa-plus pr-2'></i>{addLabel}</a>" : "";
        }

        public string ClearFiltersButton(string clearFiltersLabel)
        {
            return $"<a href='\\#' title='{clearFiltersLabel}' class='search-clear k-button' style='display: none;'><i class='far fa-times pr-2'></i><span class='label'>{clearFiltersLabel}</span></a>";
        }

        public string ToolbarTemplate(string addUrl, string addLabel, string exportToExcelLabel,bool hasExportToExcelSetup=false, string exportSetupLabel="")
        {
            return $"<nav class='nav nav-left sidebar-link'>{ExportToExcelButton(exportToExcelLabel, hasExportToExcelSetup, exportSetupLabel)}{AddButton(addUrl, addLabel)}</nav>";
        }

        // Two-add-button variant: used by unified search screens that combine a base
        // table with its _Ext sibling. The second button routes to the Ext controller's
        // Add action so records can be added to whichever underlying table is correct.
        public string ToolbarTemplateTwoAdd(string addUrl, string addLabel, string addExtUrl, string addExtLabel, string exportToExcelLabel)
        {
            return $"<nav class='nav nav-left sidebar-link'>{ExportToExcelButton(exportToExcelLabel)}{AddButton(addUrl, addLabel)}{AddButton(addExtUrl, addExtLabel)}</nav>";
        }

        public string ToolbarTemplate(string addUrl, string addLabel, string exportToExcelLabel, string clearFiltersLabel)
        {
            return $"<nav class='nav nav-left sidebar-link'>{ClearFiltersButton(clearFiltersLabel)}{ExportToExcelButton(exportToExcelLabel)}{AddButton(addUrl, addLabel)}</nav>";
        }


        public string ToolbarTemplate(IHtmlContent buttonGroupContent, string addUrl, string addLabel, string exportToExcelLabel, bool hasExportToExcelSetup = false, string exportSetupLabel = "")
        {
            string buttonGroup;

            using (var writer = new StringWriter())
            {
                buttonGroupContent.WriteTo(writer, HtmlEncoder.Default);
                buttonGroup = $@"
                        <div class=""d-flex justify-content-end search-top"">
                            {writer.ToString()}
                        </div>
                    ";
            }

            return $"<nav class='nav nav-left sidebar-link'>" +
                $"{ExportToExcelButton(exportToExcelLabel, hasExportToExcelSetup, exportSetupLabel)}" +
                $"{AddButton(addUrl, addLabel)}" +
                $"</nav>" +
                $"{buttonGroup}";

        }
    }

    public enum PageType
    {
        Search,
        SearchResults,
        Detail,
        DetailContent,
        CompactSearchResults,
        CompactSearchPage
    }
}
