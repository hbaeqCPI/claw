using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities;
using R10.Core.Entities.DMS;

namespace R10.Core.DTOs
{
    public class DMSAverageRatingDTO
    {
        [Key]
        public int DMSId { get; set; }
        public double AverageRating { get; set; }

        public Disclosure Disclosure { get; set; }
    }
}
