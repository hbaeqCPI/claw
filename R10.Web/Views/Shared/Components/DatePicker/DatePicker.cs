using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class DatePicker : ViewComponent
    {
        public DatePicker() {}

        public IViewComponentResult Invoke(DatePickerOptions model)
        {
            var pageId = model.Screen;
            if (string.IsNullOrEmpty(pageId) && ViewBag.PageId != null)
                pageId = ViewBag.PageId.ToString();

            //keep id consistent with combobox
            //var model = new DatePickerOptions { Name = name, Value = value, Id = screen + $"_{name}", Format="dd-MMM-yyyy" };
            //var model = new DatePickerOptions { Name = name, Value = value, Id = $"{name.Replace(".", "_")}_{pageId}", Format = "dd-MMM-yyyy" };
            model.Id = string.IsNullOrEmpty(model.Id) ? $"{model.Name.Replace(".", "_")}_{pageId}" : model.Id;
            model.Format = string.IsNullOrEmpty(model.Format) ? "dd-MMM-yyyy" : model.Format;

            var parseFormats =  Request.HttpContext.GetKendoDateParseFormats();
            //model.ParseFormats = parseFormats.ToArray();
            //var list = model.ParseFormats.ToList();
            //list.Add("MM dd, yyyy");
            //model.ParseFormats = list.ToArray();
            model.ParseFormats = parseFormats;
            return View(model);
        }
    }

    public class DatePickerOptions
    {
        public DatePickerOptions(string name)
        {
            Name = name;
        }

        public DatePickerOptions(string name, string onChange)
        {
            Name = name;
            OnChange = onChange;
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public string Screen { get; set; }
        public DateTime? Value { get; set; }
        public string Style { get; set; }
        public string Format { get; set; }
        public string[] ParseFormats { get; set; }
        public string ValidationMessage { get; set; }
        public string OnChange { get; set; }
    }
}


