using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ICPiUserDefaultPageManager : ICPiUserSettingManager
    {
        Task<List<CPiDefaultPage>> GetDefaultPages();
        Task<DefaultPageAction> GetDefaultPage(string userId);
        Task<DefaultPageAction> GetDefaultPageById(int pageId);
    }
}
