using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions.TagHelpers
{
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class CPiLabelTagHelper : LabelTagHelper
    {
        private const string ForAttributeName = "asp-for";

        public CPiLabelTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsRequired(For.Metadata.ValidatorMetadata))
            {
                output.AddClass("required", HtmlEncoder.Default);
            }

            base.Process(context, output);
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (IsRequired(For.Metadata.ValidatorMetadata))
            {
                output.AddClass("required", HtmlEncoder.Default);
            }
            return base.ProcessAsync(context, output);
        }

        private bool IsRequired(IReadOnlyList<object> validatorMetadata)
        {
            for (var i = 0; i < validatorMetadata.Count; i++)
            {
                if (validatorMetadata[i] is RequiredAttribute)
                {
                    if (!((RequiredAttribute)validatorMetadata[i]).AllowEmptyStrings)
                        return true;
                }
            }
            return false;
        }
    }
}
