using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatActionParameterMap : IEntityTypeConfiguration<PatActionParameter>
    {
        public void Configure(EntityTypeBuilder<PatActionParameter> builder)
        {
            builder.ToTable("tblPatActionParameter");
            builder.HasIndex(p => new { p.ActionTypeID, p.ActionDue, p.Yr, p.Mo, p.Dy }).IsUnique();
        }
    }
}
