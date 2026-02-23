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
    public class MailDownloadRuleConditionMap : IEntityTypeConfiguration<MailDownloadRuleCondition>
    {
        public void Configure(EntityTypeBuilder<MailDownloadRuleCondition> builder)
        {
            builder.ToTable("tblMailDownloadRuleCondition");
            builder.HasKey(d => new { d.Id });
            builder.HasOne(d => d.Rule).WithMany(d => d.RuleConditions).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.RuleId);
        }
    }
}
