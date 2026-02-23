using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class FavoriteMap : IEntityTypeConfiguration<MyFavorite>
    {
        public void Configure(EntityTypeBuilder<MyFavorite> builder)
        {
            builder.ToTable("tblMyFavorite");
            
        }
    }
}
