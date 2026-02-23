using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings 
{ 
    public class LetterCustomFieldMap : IEntityTypeConfiguration<LetterCustomField>
    {
        public void Configure(EntityTypeBuilder<LetterCustomField> builder)
        {
            builder.ToTable("tblLetCustomField");
            builder.HasOne(r => r.LetterDataSource).WithMany(s => s.LetterCustomFields).HasForeignKey(s => s.DataSourceId).HasPrincipalKey(s => s.DataSourceId);

        }
    }
}
