using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TLActionComparePTOMap : IEntityTypeConfiguration<TLActionComparePTO>
    {
        public void Configure(EntityTypeBuilder<TLActionComparePTO> builder)
        {
            builder.HasNoKey().HasNoKey().ToView("vwTLActionPTO_missing");
            
        }
    }
}
