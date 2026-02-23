using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;

namespace R10.Core.Services.Shared
{
    public class MyFavoriteService : IMyFavoriteService
    {
        private readonly IApplicationDbContext _repository;


        public MyFavoriteService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public IQueryable<MyFavorite> QueryableList
        {
            get
            {                
                return _repository.MyFavorites.AsNoTracking();
            }
        }

        public int GetFavoriteCount(string systemType, string dataKey, int id)
        {
            var count = _repository.MyFavorites.Count(f => f.SystemType == systemType && f.DataKey == dataKey && f.DataKeyValue == id);
            return count;
        }

        public bool IsFavorite(string systemType, string dataKey, int id, string userName)
        {
            if (_repository.MyFavorites.Any(f => f.SystemType == systemType && f.DataKey == dataKey && f.DataKeyValue == id && f.Author == userName))
                return true;

            return false;
        }

    }
}
