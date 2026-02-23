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
    public class MailDownloadLogDetailMap : IEntityTypeConfiguration<MailDownloadLogDetail>
    {
        public void Configure(EntityTypeBuilder<MailDownloadLogDetail> builder)
        {
            builder.ToTable("tblMailDownloadLogDetail");
            builder.HasKey(d => new { d.Id });
            builder.HasOne(d => d.Log).WithMany(d => d.LogDetails).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.LogId);
            builder.HasOne(d => d.Rule).WithMany(d => d.LogDetails).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.RuleId);
        }
    }
}
