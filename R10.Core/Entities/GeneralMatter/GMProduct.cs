using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMProduct : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int MatId { get; set; }

        public int OrderOfEntry { get; set; }

        public GMMatter?  Matter { get; set; }
        public Product? Product { get; set; }
    }

    

}
