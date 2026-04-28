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
    [HtmlTargetElement("input", Attributes = ForAttributeName)]
    public class CPiInputTagHelper : InputTagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName("asp-is-readonly")]
        public bool IsReadOnly { set; get; }

        [HtmlAttributeName("asp-is-autofocus")]
        public bool IsAutofocus { set; get; }

        [HtmlAttributeName("asp-can-be-posted")]
        public bool CanBePosted { set; get; }

        public CPiInputTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsReadOnly)
            {
                //use disabled. checkbox has no readonly attribute
                
                if (CanBePosted)
                   output.Attributes.Add(new TagHelperAttribute("readonly", "readonly"));
                else
                   output.Attributes.Add(new TagHelperAttribute("disabled", "disabled"));
            }

            if (IsAutofocus)
            {
                //use disabled. checkbox has no readonly attribute
                //output.Attributes.Add(new TagHelperAttribute("readonly", "readonly"));
                output.Attributes.Add(new TagHelperAttribute("autofocus", "autofocus"));
            }

            ////For.Metadata.IsRequired always returns true for non nullable data types
            //
            //if (For.Metadata.IsRequired && context.AllAttributes["required"] == null)
            if (IsRequired(For.Metadata.ValidatorMetadata) && context.AllAttributes["required"] == null)
            {
                output.Attributes.Add(new TagHelperAttribute("required"));
            }

            if (IsTradeSecret())
                output.AddClass("trade-secret", HtmlEncoder.Default);

            base.Process(context, output);
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

        private bool IsTradeSecret()
        {
            // TradeSecretAttribute removed during debloat
            return false;
        }
    }
}
