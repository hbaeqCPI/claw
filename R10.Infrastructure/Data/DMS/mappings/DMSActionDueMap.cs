using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSActionDueMap: IEntityTypeConfiguration<DMSActionDue>
    {
        public void Configure(EntityTypeBuilder<DMSActionDue> builder)
        {
            builder.ToTable("tblDMSActionDue");
            builder.HasIndex(a => new { a.DMSId, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasIndex(a => new { a.DisclosureNumber, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasOne(a => a.Disclosure).WithMany(d => d.ActionDues).HasForeignKey(a => a.DMSId).HasPrincipalKey(c => c.DMSId);
        }
    }
}
