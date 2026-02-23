using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatEPODocumentMerge : BaseEntity
    {
        [Key]
        public int MergeId { get; set; }

        [Required, Display(Name = "Map Name")]
        public string? MergeName { get; set; }

        [Required, Display(Name = "Order Of Entry")]
        public int OrderOfEntry { get; set; }

        [Required, Display(Name = "File Name")]
        public string? FileName { get; set; }

        [Display(Name = "In Use?")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// If multiple mappings/combinations apply to a single batch of downloaded documents,
        /// stop processing succeeding mappings/combinations once first converted document is completed
        /// </summary>
        [Display(Name = "Stop Processing More Combinations?")]
        public bool StopProcessing { get; set; } = false;

        /// <summary>
        /// Remove source files used for merging only after all files listed in the mapping have been merged
        /// </summary>
        [Display(Name = "Remove Source Files After Merging?")]
        public bool DeleteSourceFiles { get; set; } = false;

        public List<PatEPODocumentMergeGuide>? MergeGuides { get; set; }

        [NotMapped]
        public int GuideCount { get; set; }
    }
}
