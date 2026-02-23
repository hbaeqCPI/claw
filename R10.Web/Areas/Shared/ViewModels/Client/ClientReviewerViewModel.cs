using R9.Core.Entities;
using R9.Core.Entities.DMS;
using R9.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.Areas.Shared.ViewModels
{
    public class ClientReviewerViewModel
    {
        public int SettingId { get; set; }
        public bool IsDefaultReviewer { get; set; }

        public int OrderOfEntry { get; set; }

        public int EntityId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; }
    }
}
