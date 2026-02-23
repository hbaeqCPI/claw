using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.AMS;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSRemLogMap : IEntityTypeConfiguration<RemLog<AMSDue, AMSRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLog<AMSDue, AMSRemLogDue>> builder)
        {
            builder.ToTable("tblAMSRemLog");
            builder.HasKey(r => r.RemId);
        }
    }
}
