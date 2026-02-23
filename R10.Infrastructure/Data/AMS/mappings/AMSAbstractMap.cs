using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSAbstractMap : IEntityTypeConfiguration<AMSAbstract>
    {
        public void Configure(EntityTypeBuilder<AMSAbstract> builder)
        {
            builder.ToTable("tblAMSAbstract");
            builder.HasKey(a => a.AnnID);
        }
    }
}
