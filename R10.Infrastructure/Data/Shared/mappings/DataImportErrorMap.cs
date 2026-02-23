using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DataImportErrorMap : IEntityTypeConfiguration<DataImportError>
    {
        public void Configure(EntityTypeBuilder<DataImportError> builder)
        {
            builder.ToTable("tblDataImportErrorLog");
            
        }
    }
}
