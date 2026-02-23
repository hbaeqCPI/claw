using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatScoreMap : IEntityTypeConfiguration<PatScore>
    {
        public void Configure(EntityTypeBuilder<PatScore> builder)
        {
            builder.ToTable("tblPatScore");
            builder.HasOne(s=>s.CountryApplication).WithMany(a=>a.PatScores).HasForeignKey(s=>s.AppId).HasPrincipalKey(a => a.AppId);
            builder.HasOne(s => s.ScoreCategory).WithMany(c => c.PatScores).HasForeignKey(s => s.CategoryId).HasPrincipalKey(c => c.CategoryId);
        }
    }
}
