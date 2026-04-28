using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiUserDefaultPageManager : ICPiUserSettingManager
    {
        Task<List<CPiDefaultPage>> GetDefaultPages();
        Task<DefaultPageAction> GetDefaultPage(string userId);
        Task<DefaultPageAction> GetDefaultPageById(int pageId);
    }
}
