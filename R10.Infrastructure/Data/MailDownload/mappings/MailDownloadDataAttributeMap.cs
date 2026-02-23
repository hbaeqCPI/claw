using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.MailDownload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.MailDownload.mappings
{
    internal class MailDownloadDataAttributeMap : IEntityTypeConfiguration<MailDownloadDataAttribute>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MailDownloadDataAttribute> builder)
        {
            builder.ToTable("tblMailDownloadDataAttribute");
            builder.HasKey(d => new { d.Id });
        }
    }
}
