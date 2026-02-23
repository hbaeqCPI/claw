using R10.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.DTOs
{
    public class ActionTabDTO : BaseEntity
    {
        [Key]
        public int DDId { get; set; }

        public int ActId { get; set; }

        public string? ActionDue { get; set; }

        public DateTime DueDate { get; set; }

        public string? Indicator { get; set; }

        public DateTime? DateTaken { get; set; }

        public string? Remarks { get; set; }

        public string? Responsible { get; set; }

        public bool ComputerGenerated{ get; set; }

    }

   public enum ActionDisplayOption { Open=0, Close, All }
}
