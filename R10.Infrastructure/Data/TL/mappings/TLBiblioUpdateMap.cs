using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TLBiblioUpdateMap : IEntityTypeConfiguration<TLBiblioUpdate>
    {
        public void Configure(EntityTypeBuilder<TLBiblioUpdate> builder)
        {
            builder.HasNoKey().ToView("vwTLBiblioUpdate");
            
        }
    }
}
