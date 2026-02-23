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
    public class MailDownloadDataMapPatternMap : IEntityTypeConfiguration<MailDownloadDataMapPattern>
    {
        public void Configure(EntityTypeBuilder<MailDownloadDataMapPattern> builder)
        {
            builder.ToTable("tblMailDownloadDataMapPattern");
            builder.HasKey(d => new { d.Id });
            builder.HasIndex(d => new { d.MapId, d.Pattern }).IsUnique();
            builder.HasOne(d => d.Map).WithMany(d => d.MapPatterns).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.MapId);
        }
    }
}
