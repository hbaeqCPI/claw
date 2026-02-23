using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSDocMap : IEntityTypeConfiguration<RMSDoc>
    {
        public void Configure(EntityTypeBuilder<RMSDoc> builder)
        {
            builder.ToTable("tblRMSDoc");
            builder.HasKey(d => d.DocId);
            builder.HasIndex(d => d.Name).IsUnique();
        }
    }
}
