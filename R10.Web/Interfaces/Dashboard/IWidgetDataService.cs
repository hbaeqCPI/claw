using R10.Core.DTOs;
using R10.Web.Models.DashboardViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IWidgetDataService
    {
        Task<IEnumerable<T>> GetList<T>(UserWidgetViewModel widget) where T : class;
        Task<T> GetFirstOrDefault<T>(UserWidgetViewModel widget) where T : class;
        Task<T> GetScalar<T>(UserWidgetViewModel widget) where T : class;
    }
}
