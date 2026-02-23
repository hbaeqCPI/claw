using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkProduct : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int TmkId { get; set; }

        public int OrderOfEntry { get; set; }

        public TmkTrademark?  Trademark { get; set; }
        public Product? Product { get; set; }
    }

    

}
