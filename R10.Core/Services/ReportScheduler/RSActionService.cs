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
    public class RSActionService : EntityService<RSAction>, IRSActionService
    {
        private readonly IApplicationDbContext _repository;
        //private readonly ICPiDbContext _cpiDbContext;

        public RSActionService(
            IApplicationDbContext repository
            , ICPiDbContext cpiDbContext
            , ClaimsPrincipal user
            ) : base(cpiDbContext, user)
        {
            _repository = repository;
            //_cpiDbContext = cpiDbContext;
        }

        public IQueryable<RSAction> RSActions
        {
            get
            {
                return _repository.RSActions.AsNoTracking();
            }
        }

        public IQueryable<RSActionType> RSActionTypes
        {
            get
            {
                return _repository.RSActionTypes.AsNoTracking();
            }
        }

        public IQueryable<RSOrderByControl> RSOrderByControls
        {
            get
            {
                return _repository.RSOrderByControls.AsNoTracking();
            }
        }

        public bool AddRSAction(RSAction rSAction)
        {
            try
            {
                _repository.RSActions.Add(rSAction);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }  
        }

        public bool DeleteRSActionById(int actionId)
        {
            try
            {
                _repository.RSActions.Remove(RSActions.FirstOrDefault(c =>c.ActionId==actionId));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteRSActionByTaskId(int taskId)
        {
            try
            {
                _repository.RSActions.RemoveRange(RSActions.Where(c=>c.TaskId==taskId));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public RSAction GetRSActionById(int actionId)
        {
            return RSActions.FirstOrDefault(c => c.ActionId == actionId);
        }

        public IQueryable<RSAction> GetRSActions(int taksId)
        {
            return RSActions.Where(c => c.TaskId == taksId).AsNoTracking();
        }

        public bool UpdateRSAction(RSAction rSAction)
        {
            try
            {
                _repository.RSActions.Update(rSAction);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public virtual async Task<bool> Update(object key, string userName, IEnumerable<RSAction> updated, IEnumerable<RSAction> added, IEnumerable<RSAction> deleted)
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

            var repository = _cpiDbContext.GetRepository<RSAction>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public override async Task Add(RSAction rSAction)
        {
            //Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, CPiPermissions.FullModify, invention.RespOffice));

            //if (IsOwnerRequired || IsInventorRequired)
            //    Guard.Against.Null(null, IsOwnerRequired ? "Owner" : "Inventor");

            //await ValidateInvention(invention);
            await base.Add(rSAction);
        }

        public async Task ActionUpdate(RSAction rSAction)
        {
            if (rSAction.ActionId > 0)
            {
                _repository.RSActions.Update(rSAction);
            }
            else
            {
                _repository.RSActions.Add(rSAction);
            }

            //UpdateParentStamps(countryDue.CountryLawID, countryDue.Country, countryDue.CaseType, countryDue.UpdatedBy, countryDue.ParentTStamp);
            await _repository.SaveChangesAsync();

        }
    }
}
