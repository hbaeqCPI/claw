using LawPortal.Web.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions.TagHelpers
{
    [HtmlTargetElement("form", Attributes = ForAttributeName)]
    public class CPiDetailFormTagHelper : FormTagHelper
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        private const string ForAttributeName = "cpi-detail-form";


        [HtmlAttributeName("form-name")]
        public string FormName { set; get; }


        [HtmlAttributeName("action")]
        public string ActionMethod { set; get; }

        [HtmlAttributeName("error-message")]
        public string ErrorMessage { set; get; }

        [HtmlAttributeName("save-message")]
        public string SaveMessage { set; get; }

        [HtmlAttributeName("cancel-title")]
        public string CancelTitle { set; get; }

        [HtmlAttributeName("cancel-message")]
        public string CancelMessage { set; get; }

        [HtmlAttributeName("delete-title")]
        public string DeleteTitle { set; get; }

        [HtmlAttributeName("delete-message")]
        public string DeleteMessage { set; get; }

        [HtmlAttributeName("delete-success")]
        public string DeleteSuccess { set; get; }

        [HtmlAttributeName("print-title")]
        public string PrintTitle { set; get; }

        [HtmlAttributeName("print-message")]
        public string PrintMessage { set; get; }

        public CPiDetailFormTagHelper(IHtmlGenerator generator, 
            IStringLocalizer<SharedResource> localizer) : base(generator)
        {
            _localizer = localizer;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Antiforgery = true;

            if (string.IsNullOrEmpty(FormName))
                FormName = "detailForm";

            output.Attributes.Add(new TagHelperAttribute("id", FormName));
            output.Attributes.Add(new TagHelperAttribute("method", "post"));

            // Don't assign ActionMethod to base.Action — that property is the
            // asp-action *name* used by routing, not a URL. Feeding a full path
            // like "/Releases/Release/Save" into it makes routing URL-encode the
            // whole string and append it to the current path, producing requests
            // to "/Releases/Release/%2FReleases%2FRelease%2FSave" → 404.
            // We re-add the literal action attribute on the rendered output
            // (after base.Process, below) so the form posts to ActionMethod verbatim.

            if (string.IsNullOrEmpty(ErrorMessage))
                output.Attributes.Add(new TagHelperAttribute("data-error-message", _localizer["An error occurred. No updates were made."]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-error-message", ErrorMessage));

            if (string.IsNullOrEmpty(SaveMessage))
                output.Attributes.Add(new TagHelperAttribute("data-save-message", _localizer["Record has been saved successfully."]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-save-message", SaveMessage));

            if (string.IsNullOrEmpty(CancelTitle))
                output.Attributes.Add(new TagHelperAttribute("data-cancel-title", _localizer["Confirm Cancel Changes"]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-cancel-title", CancelTitle));

            if (string.IsNullOrEmpty(CancelMessage))
                output.Attributes.Add(new TagHelperAttribute("data-cancel-message", _localizer["Changes will not be saved. Do you want to continue?"]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-cancel-message", CancelMessage));

            if (string.IsNullOrEmpty(DeleteTitle))
                output.Attributes.Add(new TagHelperAttribute("data-delete-title", _localizer["Confirm Delete"]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-delete-title", DeleteTitle));

            if (string.IsNullOrEmpty(DeleteMessage))
                output.Attributes.Add(new TagHelperAttribute("data-delete-message", _localizer["You are about to delete this record. This operation cannot be undone."]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-delete-message", DeleteMessage));

            if (string.IsNullOrEmpty(DeleteSuccess))
                output.Attributes.Add(new TagHelperAttribute("data-delete-success", _localizer["Record has been deleted successfully."]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-delete-success", DeleteSuccess));

            if (string.IsNullOrEmpty(PrintTitle))
                output.Attributes.Add(new TagHelperAttribute("data-print-title", _localizer["Print View Options"]));
            else
                output.Attributes.Add(new TagHelperAttribute("data-print-title", PrintTitle));

            if (string.IsNullOrEmpty(PrintMessage))
                output.Attributes.Add(new TagHelperAttribute("data-print-message", "<form class='m-1 p-1' id='print-option' method='post'><div class='col-sm-12'><div class='form-group' style='text-align:right'><label for='ReportFormat'>Report Format: </label><select id='ReportFormat' asp-for='ReportFormat'><option value=0 selected>PDF</option><option value=1>Excel</option></select></div></div><div class='col-sm-12'><center><div class='form-group'><input id = 'PrintScreenOption' type='checkbox' /><label for='IDs'>Print All records from search</label></div></center></div></form>"));
            else
                output.Attributes.Add(new TagHelperAttribute("data-print-message", PrintMessage));

            output.Attributes.Remove(new TagHelperAttribute(ForAttributeName));

            base.Process(context, output);

            // After base.Process — base may have stripped or rewritten the action
            // attribute. Restore the literal URL the caller passed in.
            if (!string.IsNullOrEmpty(ActionMethod))
                output.Attributes.SetAttribute("action", ActionMethod);
        }
    }
}
