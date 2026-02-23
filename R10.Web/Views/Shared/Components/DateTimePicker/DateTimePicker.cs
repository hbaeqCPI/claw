using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class DateTimePicker : ViewComponent
    {
        public DateTimePicker() {}

        public IViewComponentResult Invoke(DateTimePickerOptions model)
        {
            var pageId = model.Screen;
            if (string.IsNullOrEmpty(pageId) && ViewBag.PageId != null)
                pageId = ViewBag.PageId.ToString();

            //keep id consistent with combobox
            //var model = new DatePickerOptions { Name = name, Value = value, Id = screen + $"_{name}", Format="dd-MMM-yyyy" };
            //var model = new DatePickerOptions { Name = name, Value = value, Id = $"{name.Replace(".", "_")}_{pageId}", Format = "dd-MMM-yyyy" };
            model.Id = string.IsNullOrEmpty(model.Id) ? $"{model.Name.Replace(".", "_")}_{pageId}" : model.Id;
            model.Format = string.IsNullOrEmpty(model.Format) ? "dd-MMM-yyyy hh:mm tt" : model.Format;

            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture;
            var dateTimeFormat = culture.DateTimeFormat;
            var dateSeparator = dateTimeFormat.DateSeparator;
            var dateFormat = dateTimeFormat.ShortDatePattern.Split(dateSeparator);

            List<string> parseFormat = new List<string>();
            parseFormat.Add(model.Format);
            parseFormat.Add("d");
            if (dateFormat[0].Substring(0,1).ToLower() == "m")
            {
                parseFormat.Add("M d");
                parseFormat.Add("M/d");
            }
            else if (dateFormat[0].Substring(0, 1).ToLower() == "d") {
                parseFormat.Add("d M");
                parseFormat.Add("d/M");
            }

            model.ParseFormats = parseFormat.ToArray();

            return View(model);
        }
    }

    public class DateTimePickerOptions
    {
        public DateTimePickerOptions(string name)
        {
            Name = name;
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


