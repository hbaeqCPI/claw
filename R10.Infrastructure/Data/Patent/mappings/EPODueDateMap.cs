using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class EPODueDateMap : IEntityTypeConfiguration<EPODueDate>
    {
        public void Configure(EntityTypeBuilder<EPODueDate> builder)
        {
            builder.ToTable("tblEPODueDate");
            builder.HasIndex(a => new { a.Procedure, a.IpOfficeCode, a.AppNumber, a.TermKey, a.DueDate }).IsUnique();
        }
    }
}
