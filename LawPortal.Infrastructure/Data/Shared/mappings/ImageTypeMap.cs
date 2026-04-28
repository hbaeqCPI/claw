using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Data.Shared.mappings
{
    public class ImageTypeMap : IEntityTypeConfiguration<ImageType>
    {
        public void Configure(EntityTypeBuilder<ImageType> builder)
        {
            builder.ToTable("tblPubImageType");
        }
    }
}



