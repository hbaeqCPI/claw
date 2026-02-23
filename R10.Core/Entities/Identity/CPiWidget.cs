using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiWidget
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string Policy { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; }

        [Required]
        [StringLength(256)]
        public string ViewName { get; set; }

        [StringLength(256)]
        public string RepositoryClassName { get; set; }

        [StringLength(256)]
        public string RepositoryMethodName { get; set; }

        [StringLength(256)]
        public string? RepositoryReturnType { get; set; }

        [StringLength(1000)]
        public string? SeriesColors { get; set; }

        public bool IsEnabled { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public bool CanExpand { get; set; }

        public bool CanExport { get; set; }

        public string? Template { get; set; }
        public string? LabelTemplate { get; set; }
        public string? TooltipTemplate { get; set; }

        public string? Settings { get; set; }

        public string SystemType { get; set; }

        public string? ExportViewModel { get; set; }

        public List<CPiUserWidget> CPiUserWidgets { get; set; }
        public List<CPiUserTypeDefaultWidget> CPiUserTypeDefaultWidgets { get; set; }

        public string SystemCategory { get; set; }

        public bool CanDrillDown { get; set; }
        public bool CanExportPpt { get; set; }
        public int? QueryId { get; set; }
        public string? Category { get; set; }
        public string? Group { get; set; }
        [StringLength(10)]
        public string? CustomWidgetType { get; set; }
        public bool? CanCustomWidgetExport { get; set; }
        public int? RecordsLimit { get; set; }
        public string? CountColumn { get; set; }
        public string? CreatorId { get; set; }
        public bool? SharedWidget { get; set; }

        public bool CanEmail { get; set; }
        public bool CanEditTitle { get; set; }

        public int RowSpan { get; set; }
    }
}
