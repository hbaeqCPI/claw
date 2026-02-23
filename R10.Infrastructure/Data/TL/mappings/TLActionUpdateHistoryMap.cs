using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TLActionUpdateHistoryMap : IEntityTypeConfiguration<TLActionUpdateHistory>
    {
        public void Configure(EntityTypeBuilder<TLActionUpdateHistory> builder)
        {
            builder.HasNoKey().ToView("vwTLUpdLogAction");
            
        }
    }
}
