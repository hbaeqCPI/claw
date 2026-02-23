using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ICPiMenuItemManager
    {
        IQueryable<CPiMenuItem> MenuItems { get; }
        Task<List<CPiMenuItem>> GetUserMenuItemsByParentIdAsync(string parentId);
        Task<List<CPiMenuItem>> GetMenuItemsByParentIdAsync(string parentId);
        Task<CPiMenuItem> GetMenuItemByIdAsync(string id);
        Task<int> GetNextSortOrder(string parentId);
        Task<CPiMenuItem> SaveMenuItem(CPiMenuItem menuItem);
        Task RemoveMenuItem(CPiMenuItem menuItem);
        Task RemoveMenuItems(List<CPiMenuItem> menuItems);
        Task MoveMenuItem(CPiMenuItem menuItem, int newIndex, string newParentId);
        Task SortMenuItems(string parentId);
    }
}
