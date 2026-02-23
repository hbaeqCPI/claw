using System;
using System.Collections.Generic;
using AutoMapper;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Queries.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using iText.Layout.Element;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Web.Extensions;
using R10.Web.Helpers;

namespace R10.Web.Services
{
    public class OuickEmailSetupViewModelService : IOuickEmailSetupViewModelService
    {
        
        private readonly IQuickEmailRepository _quickEmailRepository;
        private readonly IMapper _mapper;
        
        
        public OuickEmailSetupViewModelService(
            IQuickEmailRepository quickEmailRepository, IMapper mapper)                                
        {
            _quickEmailRepository = quickEmailRepository;
            _mapper = mapper;            
        }

        public virtual async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<QEMain> qeMains)
        {
            var list = qeMains.ProjectTo<QuickEmailSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                list = list.ApplySorting(request.Sorts);
            else
                list = list.OrderBy(qe=>qe.TemplateName);

            var ids = await list.Select(qe=>qe.QESetupID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await list.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public IQueryable<QEMain> AddCriteria(IQueryable<QEMain> templates, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {

                var templateName = mainSearchFilters.FirstOrDefault(f => f.Property == "TemplateName");
                if (templateName != null)
                {
                    templates = templates.Where(w => EF.Functions.Like(w.TemplateName, templateName.Value));
                    mainSearchFilters.Remove(templateName);
                }

                var createdBy = mainSearchFilters.FirstOrDefault(f => f.Property == "CreatedBy");
                if (createdBy != null)
                {
                    templates = templates.Where(w => EF.Functions.Like(w.CreatedBy, createdBy.Value));
                    mainSearchFilters.Remove(createdBy);
                }

                var updatedBy = mainSearchFilters.FirstOrDefault(f => f.Property == "UpdatedBy");
                if (updatedBy != null)
                {
                    templates = templates.Where(w => EF.Functions.Like(w.UpdatedBy, updatedBy.Value));
                    mainSearchFilters.Remove(updatedBy);
                }

                var dateCreatedFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DateCreatedFrom");
                var dateCreatedTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DateCreatedTo");
                if (dateCreatedFrom != null && dateCreatedTo == null)
                {
                    templates = templates.Where(a => a.DateCreated >= DateTime.Parse(dateCreatedFrom.Value));
                    mainSearchFilters.Remove(dateCreatedFrom);
                }
                else if (dateCreatedFrom == null && dateCreatedTo != null)
                {
                    templates = templates.Where(a => a.DateCreated <= DateTime.Parse(dateCreatedTo.Value));
                    mainSearchFilters.Remove(dateCreatedTo);
                }
                else if (dateCreatedFrom != null && dateCreatedTo != null)
                {
                    templates = templates.Where(a => a.DateCreated >= DateTime.Parse(dateCreatedFrom.Value) && a.DateCreated <= DateTime.Parse(dateCreatedTo.Value));
                    mainSearchFilters.Remove(dateCreatedFrom);
                    mainSearchFilters.Remove(dateCreatedTo);
                }

                var lastUpdateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "LastUpdateFrom");
                var lastUpdateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "LastUpdateTo");
                if (lastUpdateFrom != null && lastUpdateTo == null)
                {
                    templates = templates.Where(a => a.DateCreated >= DateTime.Parse(lastUpdateFrom.Value));
                    mainSearchFilters.Remove(lastUpdateFrom);
                }
                else if (lastUpdateFrom == null && lastUpdateTo != null)
                {
                    templates = templates.Where(a => a.DateCreated <= DateTime.Parse(lastUpdateTo.Value));
                    mainSearchFilters.Remove(lastUpdateTo);
                }
                else if (lastUpdateFrom != null && lastUpdateTo != null)
                {
                    templates = templates.Where(a => a.DateCreated >= DateTime.Parse(lastUpdateFrom.Value) && a.DateCreated <= DateTime.Parse(lastUpdateTo.Value));
                    mainSearchFilters.Remove(lastUpdateFrom);
                    mainSearchFilters.Remove(lastUpdateTo);
                }

                var qeCatIdFilter = mainSearchFilters.FirstOrDefault(f => f.Property == "QECatId_Search");
                if (qeCatIdFilter != null)
                {
                    templates = templates.Where(q => q.QECatId == Int32.Parse(qeCatIdFilter.Value));
                    mainSearchFilters.Remove(qeCatIdFilter);
                }

                var tag = mainSearchFilters.FirstOrDefault(f => f.Property == "Tag");
                if (tag != null)
                {
                    var tagsList = "";
                    if (tag != null)
                    {
                        var tags = tag.GetValueListForLoop();
                        if (tags.Count > 1)
                        {
                            foreach (var val in tags)
                            {
                                tagsList = tagsList + val + "~";
                            }
                        }
                    }

                    templates = templates.Where(qe => tag == null
                                                    || (string.IsNullOrEmpty(tagsList)
                                                        && qe.QETags.Any(t => EF.Functions.Like(t.Tag, tag.Value)))
                                                    || (!string.IsNullOrEmpty(tagsList)
                                                        && qe.QETags.Any(t => EF.Functions.Like(tagsList, '%' + t.Tag + '%')))
                                            );

                    mainSearchFilters.Remove(tag);
                }

                if (mainSearchFilters.Any())
                    templates = QueryHelper.BuildCriteria<QEMain>(templates, mainSearchFilters);

            }
            return templates;
        }


        public IQueryable<QEDetailView> GetQuickEmails(string systemType)
        {
            return _quickEmailRepository.GetQuickEmails(systemType);
        }
        
        public async Task<QuickEmailSetupDetailViewModel> GetQuickEmailSetupById(int id)
        {
            var entity =  _quickEmailRepository.GetQuickEmailById(id);
            var viewModel = await entity.ProjectTo<QuickEmailSetupDetailViewModel>().FirstOrDefaultAsync();
            return viewModel;
        }

        public Task<QEDetailView> GetQuickEmailByName(string systemType, string templateName)
        {
            return _quickEmailRepository.GetQuickEmailByName(systemType, templateName);
        }

        public IQueryable<QERecipient> GetRecipients(int id)
        {
            var recipients = _quickEmailRepository.GetRecipients(id);
            return recipients;
        }

        public async Task<QEDataSource> GetDataSource(string systemType, string dataSourceName)
        {
            return await _quickEmailRepository.GetDataSource(systemType, dataSourceName);
        }

    }
}
