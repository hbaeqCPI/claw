using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIREuroExchangeRateYearlyMap : IEntityTypeConfiguration<PatIREuroExchangeRateYearly>
    {
        public void Configure(EntityTypeBuilder<PatIREuroExchangeRateYearly> builder)
        {
            builder.ToTable("tblPatIREuroExchangeRateYearly");
            builder.Property(c => c.YearlyId).ValueGeneratedOnAdd();
            builder.Property(m => m.YearlyId).UseIdentityColumn();
            //builder.Property(m => m.PositionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}