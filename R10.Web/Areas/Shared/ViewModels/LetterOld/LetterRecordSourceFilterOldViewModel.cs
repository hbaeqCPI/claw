
using Microsoft.Extensions.Localization;
using R9.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R9.Core.DTOs;
using R9.Core.Entities;

namespace R9.Web.Areas.Shared.ViewModels
{
    public class LetterRecordSourceFilterOldViewModel : LetterRecordSourceFilter
    {
        public string RecordSourceMain { get; set; }
        public string RecordSourceDetail { get; set; }
    }

    public class LetterFilterOperatorOldViewModel
    {
    
        public static List<LookupDTO> BuildList(IStringLocalizer<SharedResource> localizer)
        {

            return new List<LookupDTO>
            {
                new LookupDTO  { Value = "RANGE", Text = localizer["RANGE"]},
                new LookupDTO  { Value = "IN", Text = localizer["IN"]},
                new LookupDTO  { Value = "NOT IN", Text = localizer["NOT IN"]},
                new LookupDTO  { Value = "LIKE", Text = localizer["LIKE"]},
                new LookupDTO  { Value = "=", Text = localizer["="]},
                new LookupDTO  { Value = "<>", Text = localizer["<>"]},
                new LookupDTO  { Value = "<", Text = localizer["<"]},
                new LookupDTO  { Value = ">", Text = localizer[">"]},
                new LookupDTO  { Value = "<=", Text = localizer["<="]},
                new LookupDTO  { Value = ">=", Text = localizer[">="]},
                new LookupDTO  { Value = "IS", Text = localizer["IS"]}

            };
        }


    }
}
