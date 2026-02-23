using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{

    #region "Result"
    public class PatentSearchViewModel
    {
        public string? AppId { get; set; }
        public string? Country { get; set; }
        public string? AppnoSyr { get; set; }
        public string? Appno10 { get; set; }
        public string? AppnoOrig { get; set; }
        public string? AppnoOrigHighlighted { get; set; }
        public string? AppDate { get; set; }
        public string? AppDateHighlighted { get; set; }
        public string? Abstract { get; set; }
        public string? AbstractHighlighted { get; set; }
        public string? Title { get; set; }
        public string? TitleHighlighted { get; set; }
        public string? TitleLanguage { get; set; }
        public string? Inventors { get; set; }
        public string? InventorsHighlighted { get; set; }
        public string? Owners { get; set; }
        public string? OwnersHighlighted { get; set; }
        public string? IPCs { get; set; }
        public string? CPCs { get; set; }
        public List<PatentSearchKDDocViewModel> KDDocs { get; set; }
        public List<PatentSearchPriorityViewModel> Priorities { get; set; }
        public Double? Score { get; set; }
        public string? LinkUrl { get; set; }
        public bool Selected { get; set; }
    }

    public class PatentSearchKDDocViewModel
    {
        public string? KD { get; set; }
        public string? DocnoType { get; set; }
        public string? IgnoreNum { get; set; }
        public string? PubDateType { get; set; }
        public string? IgnoreDate { get; set; }
        public string? DocDate { get; set; }
        public string? Docno10 { get; set; }

    }

    public class PatentSearchPriorityViewModel
    {
        public string? PriCc { get; set; }
        public string? PriDate { get; set; }
        public string? PriNos10 { get; set; }
    }
    #endregion

    #region "Export"
    public class PatentSearchExportBasicCriteriaViewModel
    {
        public string? SearchString { get; set; }
        public string? SearchMode { get; set; }
        public string?[] AppIds { get; set; }
    }
    public class PatentSearchExportAdvancedCriteriaViewModel
    {
        public List<PatentSearchAdvancedCriteriaViewModel> Criteria { get; set; }
        public string?[] AppIds { get; set; }
    }

    public class PatentSearchExportAdvancedSQLViewModel
    {
        public string AdvancedSQLStr { get; set; }
        public string[] AppIds { get; set; }
    }
    public class PatentSearchExportPrevResultCriteriaViewModel
    {
        public int SearchId { get; set; }
        public string? SearchMode { get; set; }
        public string?[] AppIds { get; set; }
    }

    public class PatentSearchExportViewModel
    {
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Application No.")]
        public string? AppnoOrig { get; set; }

        [Display(Name = "Filing Date")]
        public string? AppDate { get; set; }

        [Display(Name = "Abstract")]
        public string? Abstract { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Owners")]
        public string? Owners { get; set; }

        [Display(Name = "Inventors")]
        public string? Inventors { get; set; }

    }
    #endregion

    #region "Criteria"
    public class PatentSearchCriteriaViewModel
    {
        [Display(Name = "Search String:")]
        public string? SearchString { get; set; }
        // public string? Synonyms { get; set; }

    }

    public class PatentSearchAdvancedCriteriaViewModel
    {
        public int FieldId { get; set; }

        [Display(Name = "Search Term")]
        public string? Criteria { get; set; }

        //[Display(Name = "Word Match")]
        //public string? SearchModeType { get; set; }

        //[Display(Name = "Query Type")]
        //public string? QueryType { get; set; }

        public string? FieldName { get; set; }

        [Display(Name = "Field")]
        [UIHint("PatentSearchFieldDropDown")]
        public PatentSearchFieldListViewModel Field { get; set; }
    }

    public class PatentSearchBasicCriteriaViewModel
    {
        public string? SearchString { get; set; }
        public string? SearchMode { get; set; }
    }

    public class PatentSearchCriteriaLoadViewModel
    {
        public string? Property { get; set; }
        public string? Operator { get; set; }
        public object Value { get; set; }
    }

    public class PatentSearchEmailCriteriaViewModel
    {
        public string? EmailUrl { get; set; }
        public string? SystemType { get; set; }
        public string? ParentKey { get; set; }
        public List<QuickEmailMultipleViewModel> SearchIdsToSend { get; set; }
        public string? ParentTable { get; set; }
        public string? ScreenName { get; set; }
    }

    public class PatentSearchScheduledSearchViewModel
    {
        public int SearchId { get; set; }
        [Display(Name ="Search Date")]
        public DateTime SearchDate { get; set; }

        [Display(Name = "Criteria Name")]
        public string? CriteriaName { get; set; }

        [Display(Name = "New Entries?")]
        public bool HasNewEntries { get; set; }
    }

    public class PatentSearchScheduledSearchCriteriaViewModel
    {
        public string? SearchMode { get; set; }
        public string? Criteria { get; set; }
    }

    #endregion

    #region "Settings"
    public class PatentSearchSettingViewModel
    {
        [Display(Name = "Application No.")]
        public bool Appno { get; set; }

        //[Display(Name = "Filing Date")]
        //public bool AppDate { get; set; } 

        [Display(Name = "Abstract")]
        public bool Abstract { get; set; } = true;

        [Display(Name = "Title")]
        public bool Title { get; set; } = true;

        [Display(Name = "Inventors")]
        public bool Inventors { get; set; }

        [Display(Name = "Owners")]
        public bool Owners { get; set; }

        [Display(Name = "IPC")]
        public bool IPCs { get; set; }

        [Display(Name = "CPC")]
        public bool CPCs { get; set; }

        //[Display(Name = "Document Date")]
        //public bool DocDate { get; set; } 

        [Display(Name = "Document No.")]
        public bool Docno { get; set; }

        //[Display(Name = "Priority Date")]
        //public bool PriDate { get; set; } 

        [Display(Name = "Priority No.")]
        public bool PriNo { get; set; }
    }

    public class PatentSearchFieldListViewModel
    {
        public int FieldId { get; set; }
        public string? FieldName { get; set; }
        public string? FieldLabel { get; set; }
    }

    public class PatentSearchDateEntryViewModel
    {
        public string? FieldLabel { get; set; }
        public string? Operator { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    #endregion


}
