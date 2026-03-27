using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryDueMap : IEntityTypeConfiguration<TmkCountryDue>
    {
        public void Configure(EntityTypeBuilder<TmkCountryDue> builder)
        {
            builder.ToTable("tblTmkCountryDue");
            builder.HasKey(e => e.CDueId);
            builder.Ignore(e => e.FollowupAction);
            builder.Ignore(e => e.OldFollowupAction);
            builder.Ignore(e => e.RecurringDesc);
            // builder.Ignore(e => e.ParentTStamp); // Removed: ParentTStamp no longer exists
        }
    }
}
