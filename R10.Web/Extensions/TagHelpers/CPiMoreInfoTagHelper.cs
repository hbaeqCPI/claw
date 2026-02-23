using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions.TagHelpers
{
    [HtmlTargetElement("div", Attributes = ForAttributeName)]
    public class CPiMoreInfoTagHelper : TagHelper
    {
        private const string ForAttributeName = "cpi-more-info";
        private readonly IStringLocalizer<SharedResource> _localizer;

        [HtmlAttributeName("id")]
        public string Id { set; get; }

        [HtmlAttributeName("more-label")]
        public string MoreLabel { set; get; }

        [HtmlAttributeName("less-label")]
        public string LessLabel { set; get; }

        public CPiMoreInfoTagHelper(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add(new TagHelperAttribute("id", Id));
            output.Attributes.Add(new TagHelperAttribute("data-label-more", MoreLabel ?? _localizer["Show more"]));
            output.Attributes.Add(new TagHelperAttribute("data-label-less", LessLabel ?? _localizer["Show less"]));

            string classValue;
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"].Value, "more-info");
            }
            else
            {
                classValue = "more-info";
            }

            output.Attributes.SetAttribute("class", classValue);

            base.Process(context, output);
        }
    }
}
