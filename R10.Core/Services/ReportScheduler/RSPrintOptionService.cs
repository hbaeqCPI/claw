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
    public class RSPrintOptionService : EntityService<RSPrintOption>, IRSPrintOptionService
    {
        private readonly IApplicationDbContext _repository;
        //private readonly ICPiDbContext _cpiDbContext;

        public RSPrintOptionService(
            IApplicationDbContext repository
            , ICPiDbContext cpiDbContext
            , ClaimsPrincipal user
            ) : base(cpiDbContext, user)
        {
            _repository = repository;
            //_cpiDbContext = cpiDbContext;
        }

        public IQueryable<RSPrintOption> RSPrintOptions
        {
            get
            {
                return _repository.RSPrintOptions.AsNoTracking();
            }
        }

        public IQueryable<RSPrintOptionControl> RSPrintOptionControls
        {
            get
            {
                return _repository.RSPrintOptionControls.AsNoTracking();
            }
        }

        public bool AddRSPrintOption(RSPrintOption rSPrintOption)
        {
            try
            {
                _repository.RSPrintOptions.Add(rSPrintOption);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteRSPrintOptionById(int schedParamId)
        {
            try
            {
                _repository.RSPrintOptions.Remove(RSPrintOptions.FirstOrDefault(c => c.SchedParamId == schedParamId));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteRSPrintOptionByTaskId(int taskId)
        {
            try
            {
                _repository.RSPrintOptions.RemoveRange(RSPrintOptions.Where(c => c.TaskId == taskId));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public RSPrintOption GetRSPrintOptionById(int schedParamId)
        {
            return RSPrintOptions.FirstOrDefault(c => c.SchedParamId == schedParamId);
        }

        public IQueryable<RSPrintOption> GetRSPrintOptions(int taksId)
        {
            return RSPrintOptions.Where(c => c.TaskId == taksId).AsNoTracking();
        }

        public bool UpdateRSPrintOption(RSPrintOption rSPrintOption)
        {
            try
            {
                _repository.RSPrintOptions.Update(rSPrintOption);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public virtual async Task<bool> Update(object key, string userName, IEnumerable<RSPrintOption> updated, IEnumerable<RSPrintOption> added, IEnumerable<RSPrintOption> deleted)
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

            var repository = _cpiDbContext.GetRepository<RSPrintOption>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public override async Task Add(RSPrintOption rSPrintOption)
        {
            //Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, CPiPermissions.FullModify, invention.RespOffice));

            //if (IsOwnerRequired || IsInventorRequired)
            //    Guard.Against.Null(null, IsOwnerRequired ? "Owner" : "Inventor");

            //await ValidateInvention(invention);
            await base.Add(rSPrintOption);
        }
    }
}
