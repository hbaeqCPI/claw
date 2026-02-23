using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class EmailSetupService : ChildEntityService<EmailType, EmailSetup>, IChildEntityService<EmailType, EmailSetup>
    {
        public EmailSetupService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override async Task Add(EmailSetup entity)
        {
            await ValidateDefault(entity);
            await UpdateParent(entity);
            await base.Add(entity);
        }

        public override async Task Update(EmailSetup entity)
        {
            await ValidateDefault(entity);
            await UpdateParent(entity);
            await base.Update(entity);
        }

        public override async Task Delete(EmailSetup entity)
        {
            //do not allow delete default
            if (entity.Default)
                throw new Exception("Unable to delete default content.");

            //update user stamps for parent
            entity.UpdatedBy = _user.GetUserName();
            entity.LastUpdate = DateTime.Now;

            await UpdateParent(entity);
            await base.Delete(entity);
        }

        /// <summary>
        /// Ensure there's one and only one default record.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task ValidateDefault(EmailSetup entity)
        {
            if(entity.Default)
            {
                var defaultSetups = await QueryableList.Where(e => e.EmailSetupId != entity.EmailSetupId && 
                                                                   e.EmailTypeId == entity.EmailTypeId && 
                                                                   e.Default).ToListAsync();
                foreach(var defaultSetup in defaultSetups )
                {
                    var entry = _cpiDbContext.GetRepository<EmailSetup>().Attach(defaultSetup);
                    defaultSetup.Default = false;
                    defaultSetup.UpdatedBy = entity.UpdatedBy;
                    defaultSetup.LastUpdate = entity.LastUpdate;

                    //entry is sometimes not tracked 
                    //even when setting entry.State implicitly
                    //
                    //entry.State = EntityState.Modified;
                    //entry.Property(p => p.Default).IsModified = true;
                    //entry.Property(p => p.UpdatedBy).IsModified = true;
                    //entry.Property(p => p.LastUpdate).IsModified = true;
                    //
                    //had to force commit
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            else if (await QueryableList.CountAsync(e => e.EmailSetupId != entity.EmailSetupId && 
                                                        e.EmailTypeId == entity.EmailTypeId && 
                                                        e.Default) == 0)
            {
                //set default to true if entity is the only email setup record
                entity.Default = true;
            }
        }

        private async Task UpdateParent(EmailSetup entity)
        {
            EmailType parent = await _cpiDbContext.GetRepository<EmailType>().GetByIdAsync(entity.EmailTypeId);

            Guard.Against.NoRecordPermission(parent != null);

            _cpiDbContext.GetRepository<EmailType>().Attach(parent);
            parent.UpdatedBy = entity.UpdatedBy;
            parent.LastUpdate = entity.LastUpdate;
        }
    }
}
