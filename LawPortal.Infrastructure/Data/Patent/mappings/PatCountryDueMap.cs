using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatCountryDueMap : IEntityTypeConfiguration<PatCountryDue>
    {
        public void Configure(EntityTypeBuilder<PatCountryDue> builder)
        {
            builder.ToTable("tblPatCountryDue");
            builder.HasKey(e => e.CDueId);
            builder.Ignore(e => e.FollowupAction);
            builder.Ignore(e => e.OldFollowupAction);
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
            // builder.Ignore(e => e.ParentTStamp); // Removed: ParentTStamp no longer exists
        }
    }
}
