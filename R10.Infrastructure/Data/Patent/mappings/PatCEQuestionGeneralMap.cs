using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEQuestionGeneralMap : IEntityTypeConfiguration<PatCEQuestionGeneral>
    {
        public void Configure(EntityTypeBuilder<PatCEQuestionGeneral> builder)
        {
            builder.ToTable("tblPatCEQuestionGeneral");
            builder.HasIndex(c => new { c.KeyId, c.CostId }).IsUnique();
            builder.HasOne(c => c.CostEstimator).WithMany(c => c.PatCEQuestionGenerals).HasPrincipalKey(c => c.KeyId).HasForeignKey(d => d.KeyId);
        }
    }
}
