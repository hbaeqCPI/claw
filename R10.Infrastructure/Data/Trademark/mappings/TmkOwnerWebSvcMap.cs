using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkOwnerWebSvcMap : IEntityTypeConfiguration<TmkOwnerWebSvc>
    {
        public void Configure(EntityTypeBuilder<TmkOwnerWebSvc> builder)
        {
            builder.ToTable("tblTmkOwnerWebSvc");
            builder.Property(i => i.EntityId).ValueGeneratedOnAdd();
            builder.Property(i => i.EntityId).UseIdentityColumn();
        }
    }
}
