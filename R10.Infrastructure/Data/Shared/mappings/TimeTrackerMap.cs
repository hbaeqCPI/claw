using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class TimeTrackerMap : IEntityTypeConfiguration<TimeTracker>
    {
        public void Configure(EntityTypeBuilder<TimeTracker> builder)
        {
            builder.ToTable("tblTimeTracker");
            builder.HasOne(o => o.Attorney).WithMany(m => m.AttorneyTimeTrackers).HasForeignKey(f => f.AttorneyID).HasPrincipalKey(k => k.AttorneyID).IsRequired(false);
            builder.HasOne(o => o.CountryApplication).WithMany(m => m.TimeTrackers).HasForeignKey(f => f.AppId).HasPrincipalKey(k => k.AppId).IsRequired(false);
            builder.HasOne(o => o.TmkTrademark).WithMany(m => m.TimeTrackers).HasForeignKey(f => f.TmkId).HasPrincipalKey(k => k.TmkId).IsRequired(false);
            builder.HasOne(o => o.GeneralMatter).WithMany(m => m.TimeTrackers).HasForeignKey(f => f.MatId).HasPrincipalKey(k => k.MatId).IsRequired(false);
        }
    }
}
