using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryDueMap : IEntityTypeConfiguration<PatCountryDue>
    {
        public void Configure(EntityTypeBuilder<PatCountryDue> builder)
        {
            builder.ToTable("tblPatCountryDue");
            builder.HasKey(e => e.CDueId);
            builder.Ignore(e => e.FollowupAction);
            builder.Ignore(e => e.OldFollowupAction);
            // builder.Ignore(e => e.ParentTStamp); // Removed: ParentTStamp no longer exists
        }
    }
}
