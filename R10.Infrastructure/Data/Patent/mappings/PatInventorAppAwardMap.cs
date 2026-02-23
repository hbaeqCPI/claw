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
    public class PatInventorAppAwardMap : IEntityTypeConfiguration<PatInventorAppAward>
    {
        public void Configure(EntityTypeBuilder<PatInventorAppAward> builder)
        {
            builder.ToTable("tblPatInventorAppAward");
            builder.Property(m => m.AwardId).UseIdentityColumn();
            builder.HasOne(i => i.PatCountryApplication).WithMany(a => a.Awards).HasForeignKey(a => a.AppId).HasPrincipalKey(i => i.AppId);
            builder.HasOne(i => i.PatInventorAwardCriteria).WithMany(a => a.PatInventorAppAwards).IsRequired(false).HasForeignKey(a => a.AwardCriteriaId).HasPrincipalKey(i => i.AwardCriteriaId);
            
        }
    }
}
