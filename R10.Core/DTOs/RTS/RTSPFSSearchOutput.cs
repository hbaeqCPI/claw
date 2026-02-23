using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    public class RTSPFSSearchOutput
    {
        [Display(Name = "Application No.")]
        public string? AppNo { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNo { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNo { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        public DateTime? PubDate { get; set; }
        public DateTime? IssDate { get; set; }
        public DateTime? FilDate { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Abstract")]
        public string? AbstractPreview { get; set; }

        public string? Id { get; set; }

        [Display(Name = "Owner")]
        public string? OwnerName { get; set; }

        [Display(Name = "Inventor")]
        public string? Inventors { get; set; }

        [Display(Name = "PCT Date")]
        public DateTime? PCTDate { get; set; }
        public string? PCTNumber { get; set; }
        public string? ParentNumber { get; set; }

        [Display(Name = "Parent Date")]
        public DateTime? ParentDate { get; set; }

        [Display(Name = "Publication Date")]
        public string? PubDateDisplay { get; set; }
        [Display(Name = "Issue Date")]
        public string? IssDateDisplay { get; set; }
        [Display(Name = "Filing Date")]
        public string? FilDateDisplay { get; set; }
        [Display(Name = "PCT Date")]
        public string? PCTDateDisplay { get; set; }
        public string? ParentDateDisplay { get; set; }
        public string? DisplayKey { get; set; }

        public string? CaseType { get; set; }
        public string? NumberType { get; set; }
        public string? KD { get; set; }
    }

    #region "Citation"
    public class RTSPFSCitationHeader
    {
        public int AppId { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        public DateTime? PubDate { get; set; }
        public DateTime? IssDate { get; set; }
        public DateTime? FilDate { get; set; }
        public string? AppNos { get; set; }
        public string? AppDate { get; set; }

        public List<RTSPFSCited> Citeds { get; set; }
        public List<RTSPFSNPL> NPLs { get; set; }

        [Display(Name = "Publication Date")]
        public string? PubDateDisplay { get; set; }
        [Display(Name = "Issue Date")]
        public string? IssDateDisplay { get; set; }
        [Display(Name = "Filing Date")]
        public string? FilDateDisplay { get; set; }
    }

    public class RTSPFSCited
    {
        public string? AppNos { get; set; }
        public string? AppDate { get; set; }
        public int EPOPubId { get; set; }
        public int CitId { get; set; }

        [Display(Name = "Document")]
        public string? CitedNo { get; set; }
        public int CEPOPubId { get; set; }
        public string? Phase { get; set; }
        public string? Country { get; set; }
        public string? DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public string? DocKD { get; set; }
        public string? FirstInventor { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }
        public string? ComputedDocNo { get; set; }
        public string? ComputedKD { get; set; }
        public string? DocNoType { get; set; }

        [Display(Name = "Import?")]
        public bool Import { get; set; } = true;
    }

    public class RTSPFSNPL
    {
        public string? AppNos { get; set; }
        public string? AppDate { get; set; }
        public int EPOPubId { get; set; }
        public int CitId { get; set; }

        [Display(Name = "Text")]
        public string? NPLText { get; set; }

        [Display(Name = "Import?")]
        public bool Import { get; set; } = true;
    }

    #endregion

    #region "Worldwide Statistics"
    public class RTSPFSStatisticsSearchOutput
    {
        public int Id { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Applicant")]
        public string? Applicant { get; set; }

        [Display(Name = "CPC Symbol")]
        public string? IPC { get; set; }

        [Display(Name = "Applicant")]
        public string? Company { get; set; }

        [Display(Name = "Year")]
        public string? Year { get; set; }

        [Display(Name = "Count")]
        public int Count { get; set; }

        [Display(Name = "Your Data Count")]
        public int YourDataCount { get; set; }

        [Display(Name = "World Data Count")]
        public int WorldDataCount { get; set; }
    }

    public class RTSPFSStatisticsAuxSearchOutput
    {   
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
    #endregion

    #region "Patent Watch"
    public class RTSPFSPatentWatchOutput
    {
        public int AppId { get; set; }
        public int WatchId { get; set; }
        public string? Header { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNo { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNo { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNo { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Owner")]
        public string? OwnerName { get; set; }

        [Display(Name = "Status")]
        public string? CurrentStatus { get; set; }
        public string? SearchBy { get; set; }
        public string? SearchNo { get; set; }

        [Display(Name = "Latest Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Latest Legal Status Event")]
        public string? EventCode { get; set; }

        [Display(Name = "Event Desc")]
        public string? EventDesc { get; set; }

        public string? LinkUrl { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }
        public string? Remarks { get; set; }
        public string? Keywords { get; set; }
    }

    public class RTSPFSPatentWatchUsersToNotify
    {
        public int WatchId { get; set; }
        public string? Users { get; set; }
    }

    public class RTSPFSPatentWatchNotify
    {
        public int NotifyId { get; set; }
        public int WatchId { get; set; }

        [Display(Name = "User")]
        public string? Email { get; set; }
    }
    public class RTSLSDEventOutput
    {
        public int EPODocId { get; set; }
        public int LeId { get; set; }

        [Display(Name = "Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Event Code")]
        public string? EventCode { get; set; }
        public string? EventCountry { get; set; }

        [Display(Name = "Event Description")]
        public string? EventDesc { get; set; }
        public string? Favor { get; set; }
        public string? Neg { get; set; }
    }

    #endregion

}
