using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ImageTypeMap : IEntityTypeConfiguration<ImageType>
    {
        public void Configure(EntityTypeBuilder<ImageType> builder)
        {
            builder.ToTable("tblPubImageType");
        }
    }
}



