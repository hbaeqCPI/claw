using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatCountryExpMap : IEntityTypeConfiguration<PatCountryExp>
    {
        public void Configure(EntityTypeBuilder<PatCountryExp> builder)
        {
            builder.ToTable("tblPatCountryExp");
            builder.HasKey(e => e.CExpId);
            builder.Ignore(e => e.ParentTStamp);
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
