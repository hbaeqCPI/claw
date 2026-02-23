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
    public class MailDownloadRuleMap : IEntityTypeConfiguration<MailDownloadRule>
    {
        public void Configure(EntityTypeBuilder<MailDownloadRule> builder)
        {
            builder.ToTable("tblMailDownloadRule");
            builder.HasKey(d => new { d.Id });
            builder.HasIndex(d => new { d.ActionId, d.Name }).IsUnique();
            builder.HasOne(d => d.Action).WithMany(d => d.Rules).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.ActionId);
        }
    }
}
