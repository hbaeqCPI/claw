using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMActionDueMap : IEntityTypeConfiguration<GMActionDue>
    {
        public void Configure(EntityTypeBuilder<GMActionDue> builder)
        {
            builder.ToTable("tblGMActionDue");
            builder.HasIndex(a => new { a.MatId, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasIndex(a => new { a.CaseNumber, a.SubCase, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasOne(a => a.GMMatter).WithMany(c => c.ActionsDue).HasForeignKey(a => a.MatId).HasPrincipalKey(c => c.MatId);
            //builder.HasMany(a => a.Images).WithOne(i => i.GMActionDue).HasForeignKey(ai => ai.ParentId).HasPrincipalKey(a => a.ActId);
            
        }
    }
}
