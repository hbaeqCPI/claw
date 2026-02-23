//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using R10.Core.Entities;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace R10.Infrastructure.Data.Shared.mappings
//{
//    public class DOCXEntitySettingsMap : IEntityTypeConfiguration<DOCXEntitySetting>
//    {
//        public void Configure(EntityTypeBuilder<DOCXEntitySetting> builder)
//        {

//            builder.ToTable("tblDOCXEntitySetting");
//            builder.HasIndex(s => new {s.EntityType,s.EntityId, s.ContactId, s.DOCXCatId}).IsUnique();
//            builder.HasOne(s => s.DOCXCategory).WithMany(l => l.DOCXEntitySettings).HasForeignKey(l => l.DOCXCatId).HasPrincipalKey(l => l.DOCXCatId);

//        }
//    }
//}
