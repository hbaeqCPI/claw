using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using R10.Core.Entities.Patent;
using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using System.Transactions;
using R10.Core.Exceptions;
using System.Security.Claims;
namespace R10.Core.Services
{
    public class PatEPODocumentMapChildService<T> : ChildEntityService<PatEPODocumentMap, T> where T : BaseEntity
    {
        public PatEPODocumentMapChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user) 
        {
        }
        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;
                
                return queryableList;
            }
        }
        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {
            var today = DateTime.Now;
            var patEPODocumentMaps = await _cpiDbContext.GetRepository<PatEPODocumentMap>().QueryableList.Where(d => d.DocumentCode == (string)key).ToListAsync();
            Guard.Against.NoRecordPermission((patEPODocumentMaps != null && patEPODocumentMaps.Count > 0));
            if (patEPODocumentMaps != null && patEPODocumentMaps.Count > 0)
                patEPODocumentMaps.ForEach(d => { d.UpdatedBy = userName; d.LastUpdate = today; });
            foreach (var item in updated)
            {
                item.UpdatedBy = userName;
                item.LastUpdate = today;
            }
            foreach (var item in added)
            {
                item.GetType().GetProperty("DocumentCode").SetValue(item, key);
                item.CreatedBy = userName;
                item.DateCreated = today;
                item.UpdatedBy = userName;
                item.LastUpdate = today;
            }
            var repository = _cpiDbContext.GetRepository<T>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);
            await _cpiDbContext.SaveChangesAsync();
            return true;
        }        
    }
}