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
    public class RMSDueDocUploadLogMap : IEntityTypeConfiguration<RMSDueDocUploadLog>
    {
        public void Configure(EntityTypeBuilder<RMSDueDocUploadLog> builder)
        {
            builder.ToTable("tblRMSDueDocUploadLog");
            builder.HasKey(d => d.LogId);
            builder.HasOne(d => d.RMSDueDoc).WithMany(d => d.RMSDueDocsUploadLogs).HasPrincipalKey(d => d.DueDocId).HasForeignKey(d => d.DueDocId);
            builder.HasOne(d => d.DocFile).WithMany(d => d.RMSDueDocsUploadLogs).HasPrincipalKey(d => d.FileId).HasForeignKey(d => d.DocFileId);
        }
    }
}
