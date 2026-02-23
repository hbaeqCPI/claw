using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRepositoryRead<T> where T : class
    {
        IQueryable<T> QueryableList { get; }
        IQueryable<T> FromSql(string sql);
        IQueryable<T> FromSql(string sql, params object[] parameters);
    }
}
