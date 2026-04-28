using Microsoft.AspNetCore.Identity;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiUserPasswordHistoryRepository : IDisposable
    {
        Task CreateAsync(CPiUserPasswordHistory entity);
        IQueryable<CPiUserPasswordHistory> UserPasswordHistory { get; }
    }
}
