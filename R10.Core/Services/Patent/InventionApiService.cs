using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public interface IInventionApiService : IWebApiBaseService<InventionWebSvc, Invention>
    {
        IQueryable<CountryApplication> Applications { get; }
        IQueryable<PatDisclosureStatus> DisclosureStatuses { get; }
    }

    public class InventionApiService : WebApiBaseService<InventionWebSvc>, IInventionApiService
    {
        private readonly IInventionService _inventionService;
        private readonly IMultipleEntityService<Invention, PatInventorInv> _patInventorInvService;

        private bool? _hasPatentAuxModify;

        public InventionApiService(
            IInventionService inventionService,
            IMultipleEntityService<Invention, PatInventorInv> patInventorInvService, 
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _inventionService = inventionService;
            _patInventorInvService = patInventorInvService;
        }

        private bool HasPatentAuxModify
        {
            get
            {
                if (_hasPatentAuxModify == null)
                    _hasPatentAuxModify = _user.IsInRoles(SystemType.Patent, CPiPermissions.AuxiliaryModify);

                return (bool)_hasPatentAuxModify;
            }
        }

        IQueryable<Invention> IWebApiBaseService<InventionWebSvc, Invention>.QueryableList => _inventionService.QueryableList;

        public IQueryable<CountryApplication> Applications => _cpiDbContext.GetRepository<CountryApplication>().QueryableList;

        public IQueryable<PatDisclosureStatus> DisclosureStatuses => _cpiDbContext.GetRepository<PatDisclosureStatus>().QueryableList;

        public async Task<int> Add(InventionWebSvc webApiInvention, DateTime runDate)
        {
            await ValidateInvention(0, webApiInvention, true);
            return await SaveInvention(webApiInvention, new Invention(), runDate);
        }

        public async Task<List<int>> Import(List<InventionWebSvc> webApiInventions, DateTime runDate)
        {
            await ValidateInventions(webApiInventions, true);

            var invIds = new List<int>();
            foreach (var webApiInvention in webApiInventions)
            {
                invIds.Add(await SaveInvention(webApiInvention, new Invention(), runDate));
            }

            return invIds;
        }

        public async Task Update(int id, InventionWebSvc webApiInvention, DateTime runDate)
        {
            await ValidateInvention(id, webApiInvention, false);

            var invention = await _inventionService.GetByIdAsync(id);
            if (invention != null)
                await SaveInvention(webApiInvention, invention, runDate);
        }

        public async Task Update(List<InventionWebSvc> webApiInventions, DateTime runDate)
        {
            await ValidateInventions(webApiInventions, false);
            foreach (var webApiInvention in webApiInventions)
            {
                var invention = await _inventionService.QueryableList.FirstOrDefaultAsync(i => i.CaseNumber == webApiInvention.CaseNumber);
                if (invention != null)
                    await SaveInvention(webApiInvention, invention, runDate);
            }
        }

        private async Task<int> SaveInvention(InventionWebSvc webApiInvention, Invention invention, DateTime runDate)
        {
            var inventors = new List<PatInventorInv>();

            await SetData(webApiInvention, invention, inventors, runDate);

            if (invention.InvId == 0)
                await _inventionService.Add(invention);
            else
                await _inventionService.Update(invention);

            //create inv inventors
            if (inventors.Count > 0)
            {
                inventors.ForEach(inventor => inventor.InvId = invention.InvId);
                await _patInventorInvService.Add(inventors);
            }

            return invention.InvId;
        }

        private async Task ValidateInvention(int id, InventionWebSvc webApiInvention, bool forInsert)
        {
            try
            {
                await ValidateData(id, webApiInvention, forInsert);
            }
            catch (Exception ex)
            {
                throw new WebApiValidationException(FormatErrorMessage(0, ex.Message, webApiInvention.CaseNumber));
            }
        }

        private async Task ValidateInventions(List<InventionWebSvc> webApiInventions, bool forInsert)
        {
            var errors = new List<string>();
            var duplicates = webApiInventions.GroupBy(i => i.CaseNumber).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            for (int i = 0; i < webApiInventions.Count; i++)
            {
                var webApiInvention = webApiInventions[i];

                try
                {
                    await ValidateData(0, webApiInvention, forInsert);
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, webApiInvention.CaseNumber));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);
        }

        private async Task ValidateData(int id, InventionWebSvc webApiInvention, bool forInsert)
        {
            //check key fields
            Guard.Against.NullOrEmpty(webApiInvention.CaseNumber, "CaseNumber");

            //use queryable list without filters when adding new records
            //use queryable list from invention service when updating records to enforce resp office and entity filters
            var inventions = forInsert ? 
                _cpiDbContext.GetReadOnlyRepositoryAsync<Invention>().QueryableList : 
                _inventionService.QueryableList;
            var isFound = await inventions.AnyAsync(i => !(i.IsTradeSecret ?? false) && ((id != 0 && i.InvId == id) || (id == 0 && i.CaseNumber == webApiInvention.CaseNumber)));

            if (forInsert)
            {
                //check if invention already exists
                Guard.Against.RecordExists(isFound);

                //check required fields when adding new record
                if (_user.HasRespOfficeFilter(SystemType.Patent))
                    Guard.Against.NullOrEmpty(webApiInvention.RespOffice, "RespOffice");

                Guard.Against.NullOrEmpty(webApiInvention.DisclosureStatus, "DisclosureStatus");
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check lookup fields
            if (!string.IsNullOrEmpty(webApiInvention.DisclosureStatus))
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatDisclosureStatus>().QueryableList
                    .AnyAsync(s => s.DisclosureStatus == webApiInvention.DisclosureStatus), "DisclosureStatus");

            //check respoffice
            if (!string.IsNullOrEmpty(webApiInvention.RespOffice))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Patent, webApiInvention.RespOffice ?? "", CPiPermissions.FullModify), "RespOffice");

            //check pat aux permission if PatInventors need to be created
            if (webApiInvention.Inventors != null && webApiInvention.Inventors.Count > 0 && !HasPatentAuxModify)
            {
                var emails = webApiInvention.Inventors.Where(i => !string.IsNullOrEmpty(i.EMail)).Select(inv => inv.EMail?.ToLower()).Distinct().ToList();
                var inventorCount = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatInventor>().QueryableList.CountAsync(i => emails.Contains(i.EMail));

                Guard.Against.ValueNotAllowed(inventorCount == emails.Count, "Inventor");
            }
        }

        private async Task SetData(InventionWebSvc webApiInvention, Invention invention, List<PatInventorInv> inventors, DateTime runDate)
        {
            //set key fields
            invention.CaseNumber = webApiInvention.CaseNumber ?? "";

            //set required fields if values are not null or empty
            if (!string.IsNullOrEmpty(webApiInvention.DisclosureStatus)) invention.DisclosureStatus = webApiInvention.DisclosureStatus;
            if (!string.IsNullOrEmpty(webApiInvention.RespOffice)) invention.RespOffice = webApiInvention.RespOffice;

            //set text fields if values are not null or empty
            //set text fields to null if values are empty string
            if (!string.IsNullOrEmpty(webApiInvention.Title)) invention.InvTitle = webApiInvention.Title;
            else if (webApiInvention.Title == "") invention.InvTitle = null;

            //set date fields if values are not null or EmptyDate
            //set date fields to null if value is EmptyDate
            if (webApiInvention.DisclosureDate != null && webApiInvention.DisclosureDate != EmptyDate) invention.DisclosureDate = webApiInvention.DisclosureDate;
            else if (webApiInvention.DisclosureDate == EmptyDate) invention.DisclosureDate = null;

            //update user and date stamp
            invention.UpdatedBy = _user.GetUserName();
            invention.LastUpdate = runDate;
            if (invention.InvId == 0)
            {
                invention.CreatedBy = _user.GetUserName();
                invention.DateCreated = runDate;
            }

            //create inv inventors
            if (webApiInvention.Inventors != null)
            {
                foreach (var webApiInventor in webApiInvention.Inventors)
                {
                    //find inventor using email
                    var inventorId = await GetInventorID(webApiInventor, runDate);

                    //ignore duplicates
                    if (inventors.Exists(i => i.InventorID == inventorId))
                        continue;

                    //ignore existing inv inventor when updating invention
                    if (invention.InvId > 0 && 
                            (await _cpiDbContext.GetReadOnlyRepositoryAsync<PatInventorInv>()
                                .QueryableList.AnyAsync(i => i.InvId == invention.InvId && i.InventorID == inventorId)))
                        continue;

                    //create inv inventor
                    inventors.Add(new PatInventorInv()
                    {
                        InventorID = inventorId,
                        InvId = invention.InvId
                    });
                }
            }
        }
    }
}
