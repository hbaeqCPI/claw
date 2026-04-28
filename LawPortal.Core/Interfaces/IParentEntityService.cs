using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface IParentEntityService<T1, T2> : IEntityService<T1>
    {
        IChildEntityService<T1, T2> ChildService { get; }
    }
}
