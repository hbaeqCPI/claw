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
    public class MailDownloadActionFilterMap : IEntityTypeConfiguration<MailDownloadActionFilter>
    {
        public void Configure(EntityTypeBuilder<MailDownloadActionFilter> builder)
        {
            builder.ToTable("tblMailDownloadActionFilter");
            builder.HasKey(d => new { d.Id });
            builder.HasOne(d => d.Action).WithMany(d => d.ActionFilters).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.ActionId);
            builder.HasOne(d => d.Map).WithMany(d => d.ActionFilters).HasPrincipalKey(d => d.Id).HasForeignKey(d => d.MapId);
        }
    }
}
