using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkAreaCountry : BaseEntity
    {
        [Key]
        public int AreaCtryId { get; set; }

        [Required]
        public string? Country { get; set; }

        [Required]
        public int AreaID { get; set; }

        public TmkCountry? AreaCountry { get; set; }

        public TmkArea? Area { get; set; }

    }
}
