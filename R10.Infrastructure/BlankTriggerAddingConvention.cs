using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace R10.Infrastructure
{
    /// <summary>
    /// Mitigate breaking changes in EF7
    /// SQL Server tables with triggers or certain computed columns now require special EF Core configuration
    /// https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/breaking-changes#sql-server-tables-with-triggers-or-certain-computed-columns-now-require-special-ef-core-configuration
    /// </summary>
    public class BlankTriggerAddingConvention : IModelFinalizingConvention
    {
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
                if (table != null
                    && entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(table.Value) == null))
                {
                    entityType.Builder.HasTrigger(table.Value.Name + "_Trig");
                }

                foreach (var fragment in entityType.GetMappingFragments(StoreObjectType.Table))
                {
                    if (entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(fragment.StoreObject) == null))
                    {
                        entityType.Builder.HasTrigger(fragment.StoreObject.Name + "_Trig");
                    }
                }
            }
        }
    }
}
