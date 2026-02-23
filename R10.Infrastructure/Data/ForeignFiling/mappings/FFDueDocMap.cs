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
    public class FFDueDocMap : IEntityTypeConfiguration<FFDueDoc>
    {
        public void Configure(EntityTypeBuilder<FFDueDoc> builder)
        {
            builder.ToTable("tblFFDueDoc");
            builder.HasKey(d => d.DueDocId);
            builder.HasOne(d => d.PatDueDate).WithMany(d => d.FFDueDocs).HasPrincipalKey(d => d.DDId).HasForeignKey(d => d.DDId);
            builder.HasOne(d => d.FFDoc).WithMany(d => d.FFDueDocs).HasPrincipalKey(d => d.DocId).HasForeignKey(d => d.DocId);
        }
    }
}
