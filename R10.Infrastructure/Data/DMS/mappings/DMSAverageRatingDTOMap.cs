using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.DTOs;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSAverageRatingDTOMap : IEntityTypeConfiguration<DMSAverageRatingDTO>
    {
        public void Configure(EntityTypeBuilder<DMSAverageRatingDTO> builder)
        {
            builder.ToView("vwDMSRatingAverage");
            builder.HasOne(s => s.Disclosure).WithOne(c => c.AverageRating).HasForeignKey<DMSAverageRatingDTO>(s => s.DMSId).HasPrincipalKey<Disclosure>(c => c.DMSId);
        }
    }
}
