
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DataImportViewModel
    {
        public int ImportId { get; set; }
        public int DataType { get; set; }
        public string? SystemType { get; set; }
        public bool FileModified { get; set; }
    }

    public class DataUpdateViewModel
    {
        public int UpdateId { get; set; }
        public int DataType { get; set; }
        public string? SystemType { get; set; }
        public bool UpdateFileModified { get; set; }
    }

    public class DataImportTypeViewModel
    {
        [Display(Name ="Column")]
        public string? ColumnName { get; set; }

        [Display(Name = "Required?")]
        public bool Required { get; set; }

        [Display(Name = "Type")]
        public string? DataType { get; set; }

        [Display(Name = "Max Length")]
        public string? MaxLength { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    
    public class DataTypeSelectionViewModel
    {
        public string? SelectionType { get; set; }
        public List<DataImportType>? DataTypes { get; set; }
        public List<DataImportType>? UpdateDataTypes { get; set; }
    }

    public class DataImportOptionsViewModel
    {
        public bool IgnoreDupes { get; set; }
    }

    public class DataImportOptionsPortfolioViewModel: DataImportOptionsViewModel
    {
        public bool GenCountryLaw { get; set; }
        public DateTime? DueDateCutOff { get; set; }
        public bool Active { get; set; }

    }

    public class DataImportOptionsIDSViewModel : DataImportOptionsViewModel
    {
        public bool AllApplicable { get; set; } = true;
    }

    public class DataImportOptionsImageViewModel : DataUpdateOptionsViewModel 
    {
        public bool UseTrademarkName { get; set; }
    }

    public class DataUpdateOptionsViewModel
    {
        public bool IgnoreOrphans { get; set; }
        public int RemarksPosition { get; set; } = 1;
    }
    public class DataUpdateOptionsPortfolioViewModel : DataUpdateOptionsViewModel
    {
        public bool GenCountryLaw { get; set; }
        public DateTime? DueDateCutOff { get; set; }
        public bool Active { get; set; }

    }

    public class DataUpdateOptionsActionViewModel : DataUpdateOptionsViewModel
    {
        public bool GenDueDate { get; set; }
        public DateTime? DueDateCutOff { get; set; }
        public bool Active { get; set; }

    }

}
