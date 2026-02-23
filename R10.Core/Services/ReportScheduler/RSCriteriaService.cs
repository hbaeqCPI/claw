using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class RSCriteriaService: EntityService<RSCriteria>, IRSCriteriaService
    {
        private readonly IApplicationDbContext _repository;
        //private readonly ICPiDbContext _cpiDbContext;

        public RSCriteriaService(
            IApplicationDbContext repository
            , ICPiDbContext cpiDbContext
            , ClaimsPrincipal user
            ) : base(cpiDbContext, user)
        {
            _repository = repository;
            //_cpiDbContext = cpiDbContext;
        }

        public IQueryable<RSCriteria> RSCriterias
        {
            get
            {
                return _repository.RSCriterias.AsNoTracking();
            }
        }

        public IQueryable<RSCriteriaControl> RSCriteriaControls
        {
            get
            {
                return _repository.RSCriteriaControls.AsNoTracking();
            }
        }

        public bool AddRSCriteria(RSCriteria rSCriteria)
        {
            try
            {
                _repository.RSCriterias.Add(rSCriteria);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteRSCriteriaById(int schedCritId)
        {
            try
            {
                _repository.RSCriterias.Remove(RSCriterias.FirstOrDefault(c => c.SchedCritId == schedCritId));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteRSCriteriaByTaskId(int taskId)
        {
            try
            {
                _repository.RSCriterias.RemoveRange(RSCriterias.Where(c => c.TaskId == taskId));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public RSCriteria GetRSCriteriaById(int schedCritId)
        {
            return RSCriterias.FirstOrDefault(c => c.SchedCritId == schedCritId);
        }

        public IQueryable<RSCriteria> GetRSCriterias(int taksId)
        {
            return RSCriterias.Where(c => c.TaskId == taksId).AsNoTracking();
        }

        public bool UpdateRSCriteria(RSCriteria rSCriteria)
        {
            try
            {
                _repository.RSCriterias.Update(rSCriteria);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public virtual async Task<bool> Update(object key, string userName, IEnumerable<RSCriteria> updated, IEnumerable<RSCriteria> added, IEnumerable<RSCriteria> deleted)
        {
            RSMain parent = await _cpiDbContext.GetRepository<RSMain>().GetByIdAsync(key);

            Guard.Against.NoRecordPermission(parent != null);

            _cpiDbContext.GetRepository<RSMain>().Attach(parent);
            parent.UpdatedBy = userName;
            parent.LastUpdate = DateTime.Now;

            foreach (var item in updated)
            {
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            foreach (var item in added)
            {
                item.GetType().GetProperty(_cpiDbContext.GetRepository<RSMain>().PrimaryKey.Name).SetValue(item, key);
                item.CreatedBy = parent.UpdatedBy;
                item.DateCreated = parent.LastUpdate;
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            var repository = _cpiDbContext.GetRepository<RSCriteria>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }
        public override async Task Add(RSCriteria rSCriteria)
        {
            //Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, CPiPermissions.FullModify, invention.RespOffice));

            //if (IsOwnerRequired || IsInventorRequired)
            //    Guard.Against.Null(null, IsOwnerRequired ? "Owner" : "Inventor");

            //await ValidateInvention(invention);
            await base.Add(rSCriteria);
        }
    }
}
