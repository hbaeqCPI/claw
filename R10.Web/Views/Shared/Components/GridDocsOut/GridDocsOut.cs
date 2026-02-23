using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.DTOs;

namespace R10.Web.ViewComponents
{
    public class GridDocsOut : ViewComponent
    {
        public IViewComponentResult Invoke(string area, string systemType, string parentKey, int parentValue, string screenCode,string? roleLink,bool canUploadDocuments, bool canDeleteRecord)
        {
            var model = new DocsOutGridOptions
            {
                Area = string.IsNullOrEmpty(area) ? "Shared" : area,
                SystemType = systemType,
                ParentKey = parentKey,
                ParentValue = parentValue,
                ScreenCode = screenCode,
                RoleLink=roleLink,
                CanUploadDocuments = canUploadDocuments,
                CanDeleteRecord=canDeleteRecord
            };
            return View(model);
        }

    }
    public class DocsOutGridOptions
    {
        public string Area { get; set; }
        public string SystemType { get; set; }
        public string ParentKey { get; set; }
        public int ParentValue { get; set; }
        public string ScreenCode { get; set; }
        public bool CanUploadDocuments { get; set; }
        public bool CanDeleteRecord { get; set; }
        public string? RoleLink { get; set; }
    }

}
