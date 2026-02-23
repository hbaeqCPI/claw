using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridAction : ViewComponent
    {
        public IViewComponentResult Invoke(ActionGridOptions model)
        {
            return View(model);
        }
    }

    public class ActionGridOptions : GridOptions
    {
        public bool? ComputerGenerated { get; set; }
        public bool IsUserDeDocketer { get; set; }
        public bool CanUploadDueDateDocument { get; set; }
        public bool CanDownloadDueDateDocument { get; set; }
        public bool ShowDeDocketRemarksOnly { get; set; }
        public string DateTakenEditableFunction { get; set; }     // for trademark ren/due action
        public bool IsIDSAction { get; set; }
        public DetailPagePermission ActionPagePermission { get; set; }
        public bool IsDocumentVerificationOn { get; set; }
        public int? ResponsibleID { get; set; }
        public int RequestDocketPendingCount { get; set; }
        public string RequestDocketHistoryUrl { get; set; }
    }
}
