using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSLicenseeMap : IEntityTypeConfiguration<AMSLicensee>
    {
        public void Configure(EntityTypeBuilder<AMSLicensee> builder)
        {
            builder.ToTable("tblAMSLicensee");
            builder.HasIndex(l => new { l.AnnID, l.Licensee, l.Licensor }).IsUnique();
            builder.HasOne(l => l.AMSMain).WithMany(m => m.AMSLicensees).HasForeignKey(l => l.AnnID).HasPrincipalKey(m => m.AnnID);
        }
    }
}
