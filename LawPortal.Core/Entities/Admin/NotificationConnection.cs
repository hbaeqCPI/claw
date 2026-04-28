using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities
{
    public class NotificationConnection 
    {
        [Key]
        [StringLength(50)]
        public string UserName { get; set; }

        [StringLength(50)]
        public string ConnectionId { get; set; }
        public bool? Active { get; set; }
        public DateTime? ConnectedOn { get; set; }

        
    }
}
