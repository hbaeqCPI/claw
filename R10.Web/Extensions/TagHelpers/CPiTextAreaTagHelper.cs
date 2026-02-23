using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions.TagHelpers
{
    [HtmlTargetElement("textarea", Attributes = ForAttributeName)]
    public class CPiTextAreaTagHelper : TextAreaTagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName("asp-is-readonly")]
        public bool IsReadOnly { set; get; }

        public CPiTextAreaTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsReadOnly)
            {
                var d = new TagHelperAttribute("readonly", "readonly");
                output.Attributes.Add(d);
            }
            base.Process(context, output);
        }
    }
}
