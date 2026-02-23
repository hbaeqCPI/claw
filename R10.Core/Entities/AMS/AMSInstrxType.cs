using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
	public class AMSInstrxType : BaseEntity
	{
		[Key]
		public int InstructionId { get; set; }

		[Required]
		[StringLength(5)]
		public string InstructionType { get; set; } 

		[StringLength(100)]
        [Display(Name = "Description")]
        public string Description { get; set; }

		[Display(Name = "Instruction")]
		[StringLength(20)]
		public string ClientDescription { get; set; }

		[StringLength(10)]
		public string ClientApplicationStatus { get; set; }

		public Int16 OrderOfDisplay { get; set; }

		public bool Active { get; set; }

        [Display(Name = "In Use")]
        public bool InUse  { get; set; }

		public bool Reactivate  { get; set; }

		public bool Remind  { get; set; }

		public bool AutoUpdate  { get; set; }

		[Display(Name = "Hide In Portfolio Review")]
		public bool HideToClient  { get; set; }

		public List<AMSDue> ClientInstructionTypes { get; set; }

		public List<AMSDue> CPIInstructionTypes { get; set; }

		public List<AMSInstrxCPiLogDetail> SentInstructionTypes { get; set; }
		public List<AMSStatusChangeLog> TriggerInstructionTypes { get; set; }
        public List<AMSInstrxDecisionMgt> DecisionMgtInstructionTypes { get; set; }
    }

	//user editable settings
	public enum InstructionTypeSetting
	{
		InUse,
		HideToClient
	}
}
