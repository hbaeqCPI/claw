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
    public class RMSDueDocMap : IEntityTypeConfiguration<RMSDueDoc>
    {
        public void Configure(EntityTypeBuilder<RMSDueDoc> builder)
        {
            builder.ToTable("tblRMSDueDoc");
            builder.HasKey(d => d.DueDocId);
            builder.HasOne(d => d.TmkDueDate).WithMany(d => d.RMSDueDocs).HasPrincipalKey(d => d.DDId).HasForeignKey(d => d.DDId);
            builder.HasOne(d => d.RMSDoc).WithMany(d => d.RMSDueDocs).HasPrincipalKey(d => d.DocId).HasForeignKey(d => d.DocId);
        }
    }
}
