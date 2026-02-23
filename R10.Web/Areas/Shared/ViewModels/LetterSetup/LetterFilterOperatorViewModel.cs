using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Web.Models;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterFilterOperatorViewModel
    {
        public static List<LookupDTO> BuildList(IStringLocalizer<SharedResource> localizer)
        {

            return new List<LookupDTO>
            {
                new LookupDTO  { Value = "BETWEEN", Text = localizer["BETWEEN"]},
                new LookupDTO  { Value = "IN", Text = localizer["IN"]},
                new LookupDTO  { Value = "NOT IN", Text = localizer["NOT IN"]},
                new LookupDTO  { Value = "LIKE", Text = localizer["LIKE"]},
                new LookupDTO  { Value = "NOT LIKE", Text = localizer["NOT LIKE"]},             // added to match LIKE
                //new LookupDTO  { Value = "IS", Text = localizer["IS"]},                       // replace by Is Null & Is Not Null
                new LookupDTO  { Value = "IS NULL", Text = localizer["IS NULL"]},               // have to add this since browser/kendo (??) clears value 'NULL' typed into the operand1/2 combo
                new LookupDTO  { Value = "IS NOT NULL", Text = localizer["IS NOT NULL"]},       // added to match IS NULL
                new LookupDTO  { Value = "=", Text = localizer["="]},
                new LookupDTO  { Value = "<>", Text = localizer["<>"]},
                new LookupDTO  { Value = "<", Text = localizer["<"]},
                new LookupDTO  { Value = ">", Text = localizer[">"]},
                new LookupDTO  { Value = "<=", Text = localizer["<="]},
                new LookupDTO  { Value = ">=", Text = localizer[">="]}
              
            };
        }
    }
}
