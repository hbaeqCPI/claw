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
    public class MailDownloadRuleResponsibleMap : IEntityTypeConfiguration<MailDownloadRuleResponsible>
    {
        public void Configure(EntityTypeBuilder<MailDownloadRuleResponsible> builder)
        {
            builder.ToTable("tblMailDownloadRuleResponsible");
            builder.HasKey(d => new { d.Id });
            builder.HasOne(d => d.Rule).WithMany(d => d.Responsibles).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.RuleId);
        }
    }
}
