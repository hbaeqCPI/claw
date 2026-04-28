using LawPortal.Core.Entities;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Services
{
    public class ParentEntityService<T1, T2> : EntityService<T1>, IParentEntityService<T1, T2> where T1 : BaseEntity where T2 : BaseEntity
    {
        public ParentEntityService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            ChildService = new ChildEntityService<T1, T2>(cpiDbContext, user);
        }

        public virtual IChildEntityService<T1, T2> ChildService { get; }
    }
}
