using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using R10.Core.Entities;

namespace R10.Core.Queries.Shared
{

    public class QEDetailView : BaseEntity
    {
        public int QESetupID { get; set; }

        public int ScreenId { get; set; }

        public int DataSourceID { get; set; }

        public string? ScreenName { get; set; }

        public string? SystemType { get; set; }

        public string? DataSourceName { get; set; }

        public string? TemplateName { get; set; }
        public string? Remarks { get; set; }
        public bool IsDefault { get; set; }
        public bool InUse { get; set; }
        public bool CPiTemplate { get; set; }
        public string? Subject { get; set; }
        public string? Detail { get; set; }
        public string? Header { get; set; }
        public string? Footer { get; set; }

        public string? LanguageCulture { get; set; }
        public string? FilePath { get; set; }
        public string? FileExt { get; set; }
        public string? FilePrefix { get; set; }
        public string? Language { get; set; }
    }
}
