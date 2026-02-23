using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class QECustomFieldMap : IEntityTypeConfiguration<QECustomField>
    {
        public void Configure(EntityTypeBuilder<QECustomField> builder)
        {
            builder.ToTable("tblQECustomField");
            builder.HasOne(r => r.QEDataSource).WithMany(s => s.QECustomFields).HasForeignKey(s => s.DataSourceID).HasPrincipalKey(s => s.DataSourceID);

        }
    }
}
