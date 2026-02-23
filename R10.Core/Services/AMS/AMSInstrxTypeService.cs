using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.AMS;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSInstrxTypeService : BaseService<AMSInstrxType>, IAMSInstrxTypeService
    {
        public AMSInstrxTypeService(ICPiDbContext cpiDbContext) : base(cpiDbContext)
        {
        }

        public override IQueryable<AMSInstrxType> QueryableList => base.QueryableList;

        public IQueryable<AMSInstrxType> InstructionTypes => QueryableList.Where(i => i.InUse).OrderBy(i => i.OrderOfDisplay);

        public IQueryable<AMSInstrxType> TicklerInstructionTypes => InstructionTypes.Where(i => !i.HideToClient).OrderBy(i => i.OrderOfDisplay);

        public override Task Add(AMSInstrxType entity)
        {
            throw new NotImplementedException();
        }

        public override Task Delete(AMSInstrxType entity)
        {
            throw new NotImplementedException();
        }

        public override async Task<AMSInstrxType> GetByIdAsync(int entityId)
        {
            return await QueryableList.SingleOrDefaultAsync(i => i.InstructionId == entityId);
        }

        public async Task<byte[]> SaveSetting(int instructionId, InstructionTypeSetting setting, bool value, byte[] tStamp, string userName)
        {
            //Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""));

            var updated = await GetByIdAsync(instructionId);

            Guard.Against.NoRecordPermission(updated != null);

            updated.tStamp = tStamp;
            _cpiDbContext.GetRepository<AMSInstrxType>().Attach(updated);

            if (setting == InstructionTypeSetting.HideToClient)
                updated.HideToClient = value;
            else if(setting == InstructionTypeSetting.InUse)
                updated.InUse = value;

            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public override Task Update(AMSInstrxType entity)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateRemarks(AMSInstrxType entity)
        {
            throw new NotImplementedException();
        }
    }
}
