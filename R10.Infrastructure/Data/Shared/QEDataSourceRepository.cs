using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Infrastructure.Data
{
    public class QEDataSourceRepository : IQEDataSourceRepository 
    {
        private readonly IApplicationDbContext _dbContext;
        public QEDataSourceRepository(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<List<QEColumnDTO>> GetDataFields(string viewName)
        {
            //var columns = GetViewModelFields(viewName);
            //if (columns.Count == 0)
            //{
            //    var param = new SqlParameter("@ViewName", viewName);
            //    columns =  _dbContext.QEColumnDTO.FromSqlRaw("procSysQEGetDataFields @ViewName", param).AsEnumerable().OrderBy(c => c.Name).ToList();                
            //}
            var param = new SqlParameter("@ViewName", viewName);
            var columns = _dbContext.QEColumnDTO.FromSqlRaw("procSysQEGetDataFields @ViewName", param).AsEnumerable().OrderBy(c => c.Name).ToList();
            return columns;
        }

        private List<QEColumnDTO> GetViewModelFields(string viewModelName)
        {
            if (string.IsNullOrEmpty(viewModelName))
                return new List<QEColumnDTO>();

            var modelDataType = Type.GetType(viewModelName);
            if (modelDataType == null)
            {
                string assemblyQualifiedName = AppDomain.CurrentDomain.GetAssemblies()
                                    .ToList()
                                    .SelectMany(x => x.GetTypes())
                                    .Where(x => x.Name == viewModelName)
                                    .Select(x => x.AssemblyQualifiedName)
                                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(assemblyQualifiedName))
                    modelDataType = Type.GetType(assemblyQualifiedName);
            }

            if (modelDataType == null)
                return new List<QEColumnDTO>();

            return modelDataType.GetProperties()
                .Select(p => new QEColumnDTO() { Name = p.Name }).ToList();
        }


    }
}
