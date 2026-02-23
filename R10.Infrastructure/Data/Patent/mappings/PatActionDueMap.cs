using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatActionDueMap : IEntityTypeConfiguration<PatActionDue>
    {
        public void Configure(EntityTypeBuilder<PatActionDue> builder)
        {
            builder.ToTable("tblPatActionDue");
            builder.HasIndex(a => new { a.AppId, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasIndex(a => new { a.CaseNumber, a.Country, a.SubCase, a.ActionType, a.BaseDate }).IsUnique();
            builder.HasOne(a => a.CountryApplication).WithMany(c => c.ActionDues).HasForeignKey(a => a.AppId).HasPrincipalKey(c=> c.AppId);
            builder.HasOne(a => a.PatCountry).WithMany(c => c.PatActionsDue).HasForeignKey(a => a.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
