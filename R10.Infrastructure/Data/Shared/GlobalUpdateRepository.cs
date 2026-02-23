using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Interfaces;

namespace R10.Infrastructure.Data
{
    public class GlobalUpdateRepository : IGlobalUpdateRepository
    {
        protected readonly ApplicationDbContext _dbContext;

        public GlobalUpdateRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<LookupDTO>> GetUpdateFields(string systemType)
        {
            return await _dbContext.GlobalUpdateFields.Where(f => f.SystemType == systemType).OrderBy(f => f.EntryOrder)
                .Select(f => new LookupDTO { Value = f.UpdateField, Text = f.FieldDescription }).ToListAsync();
        }
    }
}
