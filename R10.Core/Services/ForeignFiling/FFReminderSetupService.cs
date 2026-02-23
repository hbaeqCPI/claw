using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Interfaces;
using R10.Core.Interfaces.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.ForeignFiling
{
    public class FFReminderSetupService : ParentEntityService<FFReminderSetup, FFReminderSetupDoc>, IFFReminderSetupService
    {
        public FFReminderSetupService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public async override Task Add(FFReminderSetup entity)
        {
            _cpiDbContext.GetRepository<FFReminderSetup>().Add(entity);
            await ValidActionType(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async override Task Update(FFReminderSetup entity)
        {
            _cpiDbContext.GetRepository<FFReminderSetup>().Update(entity);
            await ValidActionType(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task ValidActionType(FFReminderSetup entity)
        {
            //Add default instruction types
            var instrxTypeAction = await _cpiDbContext.GetRepository<FFInstrxTypeAction>().QueryableList.Where(s => s.ActionType == entity.ActionType).FirstOrDefaultAsync();
            if (instrxTypeAction == null)
            {
                var instrxTypeActionDetail = new List<FFInstrxTypeActionDetail>();
                instrxTypeActionDetail.Add(new FFInstrxTypeActionDetail()
                {
                    InstructionType = "F", //FILE
                    CreatedBy = entity.UpdatedBy,
                    DateCreated = entity.LastUpdate,
                    UpdatedBy = entity.UpdatedBy,
                    LastUpdate = entity.LastUpdate
                });
                instrxTypeActionDetail.Add(new FFInstrxTypeActionDetail()
                {
                    InstructionType = "D", //DO NOT FILE
                    CreatedBy = entity.UpdatedBy,
                    DateCreated = entity.LastUpdate,
                    UpdatedBy = entity.UpdatedBy,
                    LastUpdate = entity.LastUpdate
                });

                instrxTypeAction = new FFInstrxTypeAction()
                {
                    ActionType = entity.ActionType,
                    CreatedBy = entity.UpdatedBy,
                    DateCreated = entity.LastUpdate,
                    UpdatedBy = entity.UpdatedBy,
                    LastUpdate = entity.LastUpdate,
                    FFInstrxTypeActionDetail = instrxTypeActionDetail
                };
                _cpiDbContext.GetRepository<FFInstrxTypeAction>().Add(instrxTypeAction);
            }
        }
    }
}