using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatTaxYearMap : IEntityTypeConfiguration<PatTaxYear>
    {
        public void Configure(EntityTypeBuilder<PatTaxYear> builder)
        {
            builder.ToTable("tblPatTaxYear");
            builder.HasOne(t => t.PatTaxBase).WithMany(c => c.PatTaxesYear).HasForeignKey(t => t.TaxBID).HasPrincipalKey(c=> c.TaxBID);
        }
    }
}
