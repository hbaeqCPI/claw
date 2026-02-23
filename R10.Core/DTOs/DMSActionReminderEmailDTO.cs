using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.DTOs
{
    [Keyless]
    public class DMSActionReminderEmailDTO
    {
        public int DMSId { get; set; }
        public int ActId { get; set; }
        public int QESetupId { get; set; }
        public CPiEntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public string? Email { get; set; }
        public bool AutoAttachImages { get; set; }
        public string? Error { get; set; } = string.Empty;
    }
}
