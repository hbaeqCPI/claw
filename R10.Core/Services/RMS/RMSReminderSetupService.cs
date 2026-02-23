using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.RMS;
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
    public class RMSReminderSetupService : ParentEntityService<RMSReminderSetup, RMSReminderSetupDoc>, IRMSReminderSetupService
    {
        public RMSReminderSetupService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public async override Task Add(RMSReminderSetup entity)
        {
            _cpiDbContext.GetRepository<RMSReminderSetup>().Add(entity);
            await ValidActionType(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async override Task Update(RMSReminderSetup entity)
        {
            _cpiDbContext.GetRepository<RMSReminderSetup>().Update(entity);
            await ValidActionType(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task ValidActionType(RMSReminderSetup entity)
        {
            //Add default instruction types
            var instrxTypeAction = await _cpiDbContext.GetRepository<RMSInstrxTypeAction>().QueryableList.Where(s => s.ActionType == entity.ActionType).FirstOrDefaultAsync();
            if (instrxTypeAction == null)
            {
                var instrxTypeActionDetail = new List<RMSInstrxTypeActionDetail>();
                instrxTypeActionDetail.Add(new RMSInstrxTypeActionDetail()
                {
                    InstructionType = "F", //FILE
                    CreatedBy = entity.UpdatedBy,
                    DateCreated = entity.LastUpdate,
                    UpdatedBy = entity.UpdatedBy,
                    LastUpdate = entity.LastUpdate
                });
                instrxTypeActionDetail.Add(new RMSInstrxTypeActionDetail()
                {
                    InstructionType = "D", //DO NOT FILE
                    CreatedBy = entity.UpdatedBy,
                    DateCreated = entity.LastUpdate,
                    UpdatedBy = entity.UpdatedBy,
                    LastUpdate = entity.LastUpdate
                });

                instrxTypeAction = new RMSInstrxTypeAction()
                {
                    ActionType = entity.ActionType,
                    CreatedBy = entity.UpdatedBy,
                    DateCreated = entity.LastUpdate,
                    UpdatedBy = entity.UpdatedBy,
                    LastUpdate = entity.LastUpdate,
                    RMSInstrxTypeActionDetail = instrxTypeActionDetail
                };
                _cpiDbContext.GetRepository<RMSInstrxTypeAction>().Add(instrxTypeAction);
            }
        }
    }
}
