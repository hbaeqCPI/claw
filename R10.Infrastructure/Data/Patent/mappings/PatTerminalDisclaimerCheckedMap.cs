using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatTerminalDisclaimerCheckedMap : IEntityTypeConfiguration<PatTerminalDisclaimerChecked>
    {
        public void Configure(EntityTypeBuilder<PatTerminalDisclaimerChecked> builder)
        {
            builder.ToTable("tblPatTerminalDisclaimerChecked");
            builder.HasIndex(i => new { i.AppId }).IsUnique();
            builder.HasOne(h => h.CountryApplication).WithMany(c=>c.TerminalDisclaimerCheckeds).HasForeignKey(pi => pi.AppId).HasPrincipalKey(i => i.AppId);                    
        }
    }
}
