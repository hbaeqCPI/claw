using Microsoft.AspNetCore.Identity;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ICPiUserPasswordHistoryRepository : IDisposable
    {
        Task CreateAsync(CPiUserPasswordHistory entity);
        IQueryable<CPiUserPasswordHistory> UserPasswordHistory { get; }
    }
}
