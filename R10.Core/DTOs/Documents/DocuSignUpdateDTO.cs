using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    public class DocuSignEnvelopeUpdateDTO
    {
        public string EnvelopeId { get; set; }
        public string? Status { get; set; }
        public string? VoidedReason { get; set; }
        public List<DocuSignRecipientUpdateDTO>? Recipients { get; set; }
    }

    public class DocuSignRecipientUpdateDTO
    {
        public int RecipientId { get; set; }
        public string Status { get; set; }
        public DateTime? signedDateTime { get; set; }
        //public DateTime? deliveredDateTime { get; set; }
        public DateTime? sentDateTime { get; set; }
    }

    public class DocuSignListenerDTO
    {
        public string? AccountId { get; set; }
        public string? ImpersonatedUserId { get; set; }
        public string? ClientUrl { get; set; }
        public List<string>? EnvelopeIds { get; set; }
    }
}
