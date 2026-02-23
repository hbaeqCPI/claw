using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;

namespace R10.Core.Interfaces.Shared
{
    public interface IMyFavoriteService
    {
        IQueryable<MyFavorite> QueryableList { get; }
        int GetFavoriteCount(string systemType, string dataKey, int id);
        bool IsFavorite(string systemType, string dataKey, int id, string userName);
    }
}
