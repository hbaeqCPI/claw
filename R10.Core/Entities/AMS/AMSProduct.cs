using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSProduct : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int AnnID { get; set; }

        public int OrderOfEntry { get; set; }

        public AMSMain? AMSMain { get; set; }
        public Product? Product { get; set; }
    }
}
