using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.RMS;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using R10.Core.Interfaces.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.RMS
{
    public class RMSInstrxTypeService : AuxService<RMSInstrxType>, IRMSInstrxTypeService
    {
        public RMSInstrxTypeService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public IQueryable<RMSInstrxType> InstructionTypes => QueryableList.Where(i => i.InUse).OrderBy(i => i.OrderOfDisplay);

        public IQueryable<RMSInstrxType> TicklerInstructionTypes => InstructionTypes.Where(i => !i.HideToClient).OrderBy(i => i.OrderOfDisplay);

        public async Task<byte[]> SaveSetting(int instructionId, InstructionTypeSetting setting, bool value, byte[] tStamp, string userName)
        {
            var updated = await GetByIdAsync(instructionId);

            Guard.Against.NoRecordPermission(updated != null);

            updated.tStamp = tStamp;
            _cpiDbContext.GetRepository<RMSInstrxType>().Attach(updated);

            if (setting == InstructionTypeSetting.HideToClient)
                updated.HideToClient = value;
            else if (setting == InstructionTypeSetting.InUse)
                updated.InUse = value;
            else if (setting == InstructionTypeSetting.CloseAction)
                updated.CloseAction = value;
            else if (setting == InstructionTypeSetting.SendToAgent)
                updated.SendToAgent = value;

            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public override Task Add(RMSInstrxType entity)
        {
            //return base.Add(entity);
            throw new NotImplementedException();
        }

        public override Task Delete(RMSInstrxType entity)
        {
            //return base.Delete(entity);
            throw new NotImplementedException();
        }

        public override Task Update(RMSInstrxType entity)
        {
            //return base.Update(entity);
            throw new NotImplementedException();
        }

        public override Task UpdateRemarks(RMSInstrxType entity)
        {
            //return base.UpdateRemarks(entity);
            throw new NotImplementedException();
        }
    }
}
