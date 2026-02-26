using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatInventorDMSAwardMap : IEntityTypeConfiguration<PatInventorDMSAward>
    {
        public void Configure(EntityTypeBuilder<PatInventorDMSAward> builder)
        {
            builder.ToTable("tblPatInventorDMSAward");
            builder.Property(m => m.AwardId).UseIdentityColumn();
            // Removed during deep clean
            // builder.HasOne(i => i.Disclosure).WithMany(a => a.Awards).HasForeignKey(a => a.DMSId).HasPrincipalKey(i => i.DMSId);
            builder.HasOne(i => i.PatInventorAwardCriteria).WithMany(a => a.PatInventorDMSAwards).IsRequired(false).HasForeignKey(a => a.AwardCriteriaId).HasPrincipalKey(i => i.AwardCriteriaId);

        }
    }
}
