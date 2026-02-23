using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;
using System;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormSystemMap : IEntityTypeConfiguration<FormSystem>
    {
        public void Configure(EntityTypeBuilder<FormSystem> builder)
        {
            builder.ToTable("tblFRControlSystem");
            builder.HasOne(c => c.CPiSystem).WithMany(s => s.FormSystems).HasForeignKey(c => c.SystemType).HasPrincipalKey(s => s.Id);
        }
    }
}
