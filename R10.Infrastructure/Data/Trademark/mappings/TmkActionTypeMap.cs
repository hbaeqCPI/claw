using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;


namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkActionTypeMap : IEntityTypeConfiguration<TmkActionType>
    {
        public void Configure(EntityTypeBuilder<TmkActionType> builder)
        {
            builder.ToTable("tblTmkActionType");
            builder.HasIndex(a => new { a.ActionType, a.Country, a.CDueId }).IsUnique();
            builder.HasMany(a => a.ActionParameters).WithOne(p => p.ActionType);
            // builder.HasOne(a => a.Responsible).WithMany(r => r.AttorneyTmkActionTypes).HasForeignKey(a => a.ResponsibleID).HasPrincipalKey(a => a.AttorneyID); // Removed: Responsible (Attorney) nav property no longer exists
            builder.HasOne(a => a.TmkCountry).WithMany(c => c.TmkActionTypes).HasForeignKey(t => t.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
