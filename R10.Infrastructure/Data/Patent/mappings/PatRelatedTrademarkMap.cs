using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatRelatedTrademarkMap : IEntityTypeConfiguration<PatRelatedTrademark>
    {
        public void Configure(EntityTypeBuilder<PatRelatedTrademark> builder)
        {
            builder.ToTable("tblPatRelatedTrademark");
            builder.HasOne(r=>r.Application).WithMany(c=> c.RelatedTrademarks).HasForeignKey(r => r.AppId).HasPrincipalKey(c => c.AppId);
            builder.HasOne(r => r.Trademark).WithMany(t => t.PatRelatedTrademarks).HasForeignKey(r => r.TmkId).HasPrincipalKey(c => c.TmkId);
            
        }
    }
}
