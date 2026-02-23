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
    public class FFDueDocUploadLogMap : IEntityTypeConfiguration<FFDueDocUploadLog>
    {
        public void Configure(EntityTypeBuilder<FFDueDocUploadLog> builder)
        {
            builder.ToTable("tblFFDueDocUploadLog");
            builder.HasKey(d => d.LogId);
            builder.HasOne(d => d.FFDueDoc).WithMany(d => d.FFDueDocUploadLogs).HasPrincipalKey(d => d.DueDocId).HasForeignKey(d => d.DueDocId);
            builder.HasOne(d => d.DocFile).WithMany(d => d.FFDueDocsUploadLogs).HasPrincipalKey(d => d.FileId).HasForeignKey(d => d.DocFileId);
        }
    }
}
