using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterEntitySettingsMap : IEntityTypeConfiguration<LetterEntitySetting>
    {
        public void Configure(EntityTypeBuilder<LetterEntitySetting> builder)
        {

            builder.ToTable("tblLetEntitySetting");
            builder.HasIndex(s => new {s.EntityType,s.EntityId, s.ContactId, s.LetCatId}).IsUnique();
            builder.HasOne(s => s.LetterCategory).WithMany(l => l.LetterEntitySettings).HasForeignKey(l => l.LetCatId).HasPrincipalKey(l => l.LetCatId);

        }
    }
}
