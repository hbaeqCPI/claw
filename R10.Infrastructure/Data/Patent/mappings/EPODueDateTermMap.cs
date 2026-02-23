using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class EPODueDateTermMap : IEntityTypeConfiguration<EPODueDateTerm>
    {
        public void Configure(EntityTypeBuilder<EPODueDateTerm> builder)
        {
            builder.ToTable("tblEPODueDateTerm");
            builder.HasIndex(a => new { a.TermKey }).IsUnique();
        }
    }
}
