using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSMapActionDue:BaseEntity
    {
        [Key]
        public int MapDueId { get; set; }
        public string? MapCountry { get; set; }
        public string? MapSearchAction { get; set; }

		[Display(Name ="Your Action Type")]
		[Required]
		public string? PMSActionType { get; set; }

		[Display(Name = "Your Action Due")]
		[Required]
		public string? PMSActionDue { get; set; }

		[Display(Name = "Display?")]
		public bool IncludeDisplay { get; set; }

		[Display(Name = "Compare?")]
		public bool IncludeCompare { get; set; }

		[Display(Name = "Update?")]
		public bool IncludeUpdate { get; set; }

		public Int16 DateAllowance { get; set; }

		[Display(Name = "Yr")]
		public int Yr { get; set; }

		[Display(Name = "Mo")]
		public int Mo { get; set; }

		[Display(Name = "Dy")]
		public int Dy { get; set; }

		[Display(Name = "Indicator")]
		[Required]
		public string? Indicator { get; set; }

		public string? MapGroup { get; set; }
		public int MapSourceId { get; set; }

    }

}
