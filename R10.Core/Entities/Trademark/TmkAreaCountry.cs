using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkAreaCountry : BaseEntity
    {
        [StringLength(10)]
        [Required, Display(Name = "Area")]
        public TmkArea? Area { get; set; }

        [StringLength(5)]
        [Required, Display(Name = "Country")]
        public string? Country { get; set; }
    }
}
