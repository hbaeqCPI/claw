using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using LawPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions.TagHelpers
{
    [HtmlTargetElement("i", Attributes = ForAttributeName)]
    public class CPiButtonLinkTagHelper : TagHelper
    {
        private const string ForAttributeName = "cpi-button-link";

        [HtmlAttributeName("url")]
        public string Url { set; get; }

        [HtmlAttributeName("url-data")]
        public string UrlData { set; get; }

        [HtmlAttributeName("icon")]
        public string Icon { set; get; }

        public CPiButtonLinkTagHelper()
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add(new TagHelperAttribute("data-url", Url));
            output.Attributes.Add(new TagHelperAttribute("data-url-data", UrlData));

            var icon = string.IsNullOrEmpty(Icon) ? "fal fa-search-plus" : Icon;
            var classValue = $"btn btn-link cpiButtonLink {icon}";

            if (output.Attributes.ContainsName("class"))
                classValue = $"{output.Attributes["class"].Value} {classValue}";

            output.Attributes.SetAttribute("class", classValue);

            base.Process(context, output);
        }
    }
}
