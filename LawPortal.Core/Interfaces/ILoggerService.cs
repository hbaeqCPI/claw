using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ILoggerService<T>
    {
        IQueryable<T> QueryableList { get; }
        Task<T> GetByIdAsync(int id);
        Task Add(T entity);
    }

    public interface ILoggerService<T, TContext> : ILoggerService<T> where TContext : DbContext
    {
        TContext Context { get; }
    }
}
