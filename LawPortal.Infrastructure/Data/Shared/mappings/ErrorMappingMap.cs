using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Data.Shared.mappings
{
    public class ErrorMappingMap : IEntityTypeConfiguration<ErrorMapping>
    {
        public void Configure(EntityTypeBuilder<ErrorMapping> builder)
        {
            builder.ToTable("tblPubErrorMapping");
     
        }
    }
}
