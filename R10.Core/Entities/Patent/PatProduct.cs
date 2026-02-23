using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatProduct : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int AppId { get; set; }

        public int OrderOfEntry { get; set; }

        public CountryApplication?  Application { get; set; }
        public Product? Product { get; set; }
}
}
