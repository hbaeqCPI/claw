using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DataImportTypeColumnMap : IEntityTypeConfiguration<DataImportTypeColumn>
    {
        public void Configure(EntityTypeBuilder<DataImportTypeColumn> builder)
        {
            builder.HasNoKey().ToView("vwDataImportTypeColumns");
            
        }
    }

   
}
