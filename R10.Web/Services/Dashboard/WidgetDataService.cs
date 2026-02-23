using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using R10.Core.DTOs;
using R10.Core.Interfaces;
using R10.Web.Interfaces;
using R10.Web.Models.DashboardViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class WidgetDataService : IWidgetDataService
    {
        protected ICPiDbContext _dbContext { get; }

        public WidgetDataService(ICPiDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<T>> GetList<T>(UserWidgetViewModel widget) where T : class
        {
            try
            {
                if (!widget.IsExport)
                {
                    if(widget.CPiUserWidget.CPiWidget.QueryId == null)
                    {
                        var data = await _dbContext.Query<T>()
                                        .FromSqlInterpolated($"{widget.CPiUserWidget.CPiWidget.RepositoryMethodName} @UserId={widget.CPiUserWidget.UserId}, @WidgetId={widget.CPiUserWidget.WidgetId}")
                                        .AsNoTracking()
                                        .ToListAsync();
                        return data;
                    }
                    else
                    {
                        var data = await _dbContext.Query<T>()
                                        .FromSqlInterpolated($"{widget.CPiUserWidget.CPiWidget.RepositoryMethodName} @QueryId ={widget.CPiUserWidget.CPiWidget.QueryId}, @UserId={widget.CPiUserWidget.UserId}, @HasRespOfficeOn ={widget.HasRespOffice},@HasEntityFilterOn ={widget.EntityFilterType!=Core.Identity.CPiEntityType.None}, @Category = {widget.CPiUserWidget.CPiWidget.Category},@WidgetType={widget.CPiUserWidget.CPiWidget.CustomWidgetType}, @Group={widget.CPiUserWidget.CPiWidget.Group}, @RecordsLimit={widget.CPiUserWidget.CPiWidget.RecordsLimit}, @CountColumn={widget.CPiUserWidget.CPiWidget.CountColumn}")
                                        .AsNoTracking()
                                        .ToListAsync();
                        return data;
                    }
                    
                }
                else
                {
                    var data = await _dbContext.Query<T>()
                                        .FromSqlInterpolated($"{widget.CPiUserWidget.CPiWidget.RepositoryMethodName} @UserId={widget.CPiUserWidget.UserId}, @WidgetId={widget.CPiUserWidget.WidgetId}, @IsExport={widget.IsExport}")
                                        .AsNoTracking()
                                        .ToListAsync();
                    return data;
                }
                
            }
            catch (Exception e)
            {
                var err = e.Message;

                return null;
            }

        }

        public async Task<T> GetFirstOrDefault<T>(UserWidgetViewModel widget) where T : class
        {
            try
            {
                if (widget.CPiUserWidget.CPiWidget.QueryId == null)
                {
                    var data = await _dbContext.Query<T>()
                                    .FromSqlInterpolated($"{widget.CPiUserWidget.CPiWidget.RepositoryMethodName} @UserId={widget.CPiUserWidget.UserId}, @WidgetId={widget.CPiUserWidget.WidgetId}")
                                    .AsNoTracking()
                                    .ToListAsync();
                    return data.FirstOrDefault();
                }
                else
                {
                    var data = await _dbContext.Query<T>()
                                    .FromSqlInterpolated($"{widget.CPiUserWidget.CPiWidget.RepositoryMethodName} @QueryId ={widget.CPiUserWidget.CPiWidget.QueryId}, @UserId={widget.CPiUserWidget.UserId}, @HasRespOfficeOn ={widget.HasRespOffice},@HasEntityFilterOn ={widget.EntityFilterType != Core.Identity.CPiEntityType.None}, @Category = {widget.CPiUserWidget.CPiWidget.Category},@WidgetType={widget.CPiUserWidget.CPiWidget.CustomWidgetType}, @Group={widget.CPiUserWidget.CPiWidget.Group}, @RecordsLimit={widget.CPiUserWidget.CPiWidget.RecordsLimit}, @CountColumn={widget.CPiUserWidget.CPiWidget.CountColumn}")
                                    .AsNoTracking()
                                    .ToListAsync();
                    return data.FirstOrDefault();
                }                                        
            }
            catch (Exception e)
            {
                var err = e.Message;

                return await Task.FromResult<T>(default(T));
            }
        }

        public async Task<T> GetScalar<T>(UserWidgetViewModel widget) where T : class
        {
            try
            {
                DbCommand cmd = _dbContext.GetDbConnection().CreateCommand();

                cmd.CommandText = widget.CPiUserWidget.CPiWidget.RepositoryMethodName;

                cmd.Parameters.Add(new SqlParameter("@UserId", widget.CPiUserWidget.UserId));
                cmd.Parameters.Add(new SqlParameter("@WidgetId", widget.CPiUserWidget.WidgetId));

                return (T)await cmd.ExecuteScalarAsync();
            }
            catch (Exception e)
            {
                var err = e.Message;

                return await Task.FromResult<T>(default(T));
            }
        }
    }
}
