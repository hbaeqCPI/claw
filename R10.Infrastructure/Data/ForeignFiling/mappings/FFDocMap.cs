using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFDocMap : IEntityTypeConfiguration<FFDoc>
    {
        public void Configure(EntityTypeBuilder<FFDoc> builder)
        {
            builder.ToTable("tblFFDoc");
            builder.HasKey(d => d.DocId);
            builder.HasIndex(d => d.Name).IsUnique();
        }
    }
}
