using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiUserTypeDefaultPageMap : IEntityTypeConfiguration<CPiUserTypeDefaultPage>
    {
        public void Configure(EntityTypeBuilder<CPiUserTypeDefaultPage> builder)
        {
            builder.ToTable("tblCPiUserTypeDefaultPage");
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.UserType).IsUnique(false);
        }
    }
}
