using LawPortal.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiMenuPageManager
    {
        IQueryable<CPiMenuPage> MenuPages { get; }
        Task<List<CPiMenuPage>> GetAllowedMenuPagesByAreaAsync(string areaId);
        Task<CPiMenuPage> GetMenuPageByIdAsync(int id);
        Task<CPiMenuPage> SaveMenuPage(CPiMenuPage page);
        Task RemoveMenuPage(CPiMenuPage page);
    }
}
