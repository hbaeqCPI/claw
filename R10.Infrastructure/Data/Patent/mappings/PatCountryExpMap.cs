using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryExpMap : IEntityTypeConfiguration<PatCountryExp>
    {
        public void Configure(EntityTypeBuilder<PatCountryExp> builder)
        {
            builder.ToTable("tblPatCountryExp");
            builder.HasKey(e => e.CExpId);
            builder.Ignore(e => e.ParentTStamp);
        }
    }
}
