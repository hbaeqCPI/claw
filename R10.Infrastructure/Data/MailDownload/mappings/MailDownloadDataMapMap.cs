using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.MailDownload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.MailDownload.mappings
{
    public class MailDownloadDataMapMap : IEntityTypeConfiguration<MailDownloadDataMap>
    {
        public void Configure(EntityTypeBuilder<MailDownloadDataMap> builder)
        {
            builder.ToTable("tblMailDownloadDataMap");
            builder.HasKey(d => new { d.Id });
            builder.HasIndex(d => d.Name).IsUnique();
            builder.HasOne(d => d.Attribute).WithMany(d => d.MailDownloadDataMaps).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.AttributeId);
        }
    }
}
