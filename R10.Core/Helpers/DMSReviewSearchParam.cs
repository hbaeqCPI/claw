using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core
{
    public class DMSReviewSearchParam
    {
        public string UserId { get; set; }

        public string DisclosureNumber { get; set; }

        public int DMSReviewerID { get; set; }

        public int ClientID { get; set; }

        public int OwnerID { get; set; }

        public int AttorneyID { get; set; }

        public DateTime? FromSubmittedDate { get; set; }

        public DateTime? ToSubmittedDate { get; set; }

        public int? OptionRated { get; set; }

        public string Recommendation { get; set; }

        public DateTime? FromRatingDate { get; set; }

        public DateTime? ToRatingDate { get; set; }

        public string Keyword { get; set; }

        

    }
}
