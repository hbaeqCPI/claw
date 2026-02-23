using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCEQuestionGeneralMap : IEntityTypeConfiguration<TmkCEQuestionGeneral>
    {
        public void Configure(EntityTypeBuilder<TmkCEQuestionGeneral> builder)
        {
            builder.ToTable("tblTmkCEQuestionGeneral");
            builder.HasIndex(c => new { c.KeyId, c.CostId }).IsUnique();
            builder.HasOne(c => c.CostEstimator).WithMany(c => c.TmkCEQuestionGenerals).HasPrincipalKey(c => c.KeyId).HasForeignKey(d => d.KeyId);
        }
    }
}
