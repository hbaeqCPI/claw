using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Security;
using R10.Web.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared")]
    public class MyFavoriteController : BaseController
    {
        private readonly IApplicationDbContext _repository;

        public MyFavoriteController(IApplicationDbContext repository)
        {
            _repository = repository;
        }


        public async Task<IActionResult> UpdateFavoriteCount(string systemType,string dataKey, int id)
        {
            var userName = User.GetEmail();
            var added = true;
            var favorite = await _repository.MyFavorites.Where(f => f.SystemType==systemType && f.DataKey == dataKey && f.DataKeyValue == id && f.Author == userName).FirstOrDefaultAsync();
            if (favorite != null)
            {
                _repository.MyFavorites.Remove(favorite);
                added = false;
            }
            else { 
                favorite = new MyFavorite { 
                    SystemType= systemType,
                    DataKey=dataKey,
                    DataKeyValue=id,
                    Author=userName,
                };
                UpdateEntityStamps(favorite, 0);
                _repository.MyFavorites.Add(favorite);
            }
            await _repository.SaveChangesAsync();

            var count = _repository.MyFavorites.Count(f => f.SystemType == systemType && f.DataKey == dataKey && f.DataKeyValue == id);
            return Json(new { added,count });
        }

       


    }
}