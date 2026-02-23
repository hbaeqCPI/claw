using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSAgenda: DMSAgendaDetail
    {
        
        public Client? Client { get; set; }

        public PatArea? Area { get; set; }

        public List<DMSAgendaRelatedDisclosure>? DMSAgendaRelatedDisclosures { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class DMSAgendaDetail : BaseEntity
    {
        [Key]
        public int AgendaId { get; set; }

        [Required]
        [Display(Name = "Meeting Date")]
        public DateTime? MeetingDate { get; set; }

        public int? ClientID { get; set; }

        public int? AreaID { get; set; }

        public string? Remarks { get; set; }
    }
}
