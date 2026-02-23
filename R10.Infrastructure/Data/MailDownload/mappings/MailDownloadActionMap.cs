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
    public class MailDownloadActionMap : IEntityTypeConfiguration<MailDownloadAction>
    {
        public void Configure(EntityTypeBuilder<MailDownloadAction> builder)
        {
            builder.ToTable("tblMailDownloadAction");
            builder.HasKey(d => new { d.Id });
            builder.HasIndex(d => d.Name).IsUnique();
        }
    }
}
