using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using R10.Core.Services;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.DMS;
//using R10.Web.Areas.DMS.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class DMSServiceCollectionExtensions
    {
        public static IServiceCollection AddDisclosure(this IServiceCollection services)
        {
            services.AddScoped<IAsyncRepository<DMSReview>, EFRepository<DMSReview>>();
            services.AddScoped<IAsyncRepository<DMSValuation>, EFRepository<DMSValuation>>();
            services.AddScoped<IAsyncRepository<CPiUser>, EFRepository<CPiUser>>();

            // main disclosure
            services.AddScoped<IDisclosureService, DisclosureService>();
            services.AddScoped<IDisclosureViewModelService, R10.Web.Services.Stubs.DisclosureViewModelServiceStub>(); // Stub: DMS Web layer removed

            //services.AddScoped<IDMSImageViewModelService, DMSImageViewModelService>(); // Removed: Web Area service
            services.AddScoped<IChildEntityService<Disclosure, DMSKeyword>, DMSDisclosureChildService<DMSKeyword>>();
            services.AddScoped<IChildEntityService<Disclosure, InventionRelatedDisclosure>, DMSDisclosureChildService<InventionRelatedDisclosure>>();
            services.AddScoped<IChildEntityService<Disclosure, DisclosureRelatedDisclosure>, DMSDisclosureChildService<DisclosureRelatedDisclosure>>();
            services.AddScoped<IChildEntityService<Disclosure, DMSQuestion>, DMSDisclosureChildService<DMSQuestion>>();
            services.AddScoped<IChildEntityService<Disclosure, DMSDiscussion>, DMSDisclosureChildService<DMSDiscussion>>();
            services.AddScoped<IParentEntityService<DMSDiscussion, DMSDiscussionReply>, ParentEntityService<DMSDiscussion, DMSDiscussionReply>>();

            services.AddScoped<IDMSInventorService, DMSInventorService>();
            services.AddScoped<IEntityService<DMSInventor>, AuxService<DMSInventor>>();
            services.AddScoped<IEntityService<PatInventorDMSAward>, AuxService<PatInventorDMSAward>>();
            services.AddScoped<IParentEntityService<Disclosure, PatInventorDMSAward>, ParentEntityService<Disclosure, PatInventorDMSAward>>();
            services.AddScoped<IParentEntityService<PatInventor, PatInventorDMSAward>, ParentEntityService<PatInventor, PatInventorDMSAward>>();

            // actions
            //services.AddScoped<IDMSActionDueViewModelService, DMSActionDueViewModelService>(); // Removed: Web Area service
            services.AddScoped<IActionDueService<DMSActionDue, DMSDueDate>, DMSActionDueService>();
            services.AddScoped<IDueDateService<DMSActionDue, DMSDueDate>, DMSDueDateService>();
            services.AddScoped<IEntityService<DMSDueDateDelegation>, AuxService<DMSDueDateDelegation>>();

            // preview
            //services.AddScoped<IDisclosurePreviewViewModelService, DisclosurePreviewViewModelService>(); // Removed: Web Area service
            services.AddScoped<IDMSPreviewService, DMSPreviewService>();

            // reviews
            services.AddScoped<IDMSReviewService, DMSReviewService>();
            //services.AddScoped<IDisclosureReviewViewModelService, DisclosureReviewViewModelService>(); // Removed: Web Area service

            // auxiliary
            services.AddScoped<IViewModelService<DMSIndicator>, ViewModelService<DMSIndicator>>();
            services.AddScoped<IEntityService<DMSIndicator>, AuxService<DMSIndicator>>();

            services.AddScoped<IViewModelService<DMSActionType>, ViewModelService<DMSActionType>>();
            services.AddScoped<IParentEntityService<DMSActionType, DMSActionParameter>, ParentEntityService<DMSActionType, DMSActionParameter>>();

            services.AddScoped<IViewModelService<DMSDisclosureStatus>, ViewModelService<DMSDisclosureStatus>>();
            services.AddScoped<IEntityService<DMSDisclosureStatus>, AuxService<DMSDisclosureStatus>>();

            services.AddScoped<IViewModelService<DMSRating>, ViewModelService<DMSRating>>();
            services.AddScoped<IEntityService<DMSRating>, AuxService<DMSRating>>();

            services.AddScoped<IViewModelService<DMSRecommendation>, ViewModelService<DMSRecommendation>>();
            services.AddScoped<IEntityService<DMSRecommendation>, AuxService<DMSRecommendation>>();

            // workflow
            services.AddScoped<IDMSWorkflowService, DMSWorkflowService>();
            services.AddScoped<IAsyncRepository<DMSWorkflow>, EFRepository<DMSWorkflow>>();
            services.AddScoped<IViewModelService<DMSWorkflow>, ViewModelService<DMSWorkflow>>();

            // questionnaire
            services.AddScoped<IDMSQuestionnaireService, DMSQuestionnaireService>();
            services.AddScoped<IAsyncRepository<DMSQuestionGroup>, EFRepository<DMSQuestionGroup>>();
            services.AddScoped<IViewModelService<DMSQuestionGroup>, ViewModelService<DMSQuestionGroup>>();

            // valuation matrix
            services.AddScoped<IDMSValuationMatrixService, DMSValuationMatrixService>();
            services.AddScoped<IAsyncRepository<DMSValuationMatrix>, EFRepository<DMSValuationMatrix>>();
            services.AddScoped<IViewModelService<DMSValuationMatrix>, ViewModelService<DMSValuationMatrix>>();

            services.AddScoped<IDMSEntityReviewerService, DMSEntityReviewerService>();

            //settings
            services.AddScoped<ISystemSettings<DMSSetting>, SystemSettings<DMSSetting>>();

            // agenda meeting
            services.AddScoped<IDMSAgendaService, DMSAgendaService>();
            //services.AddScoped<IDMSAgendaViewModelService, DMSAgendaViewModelService>(); // Removed: Web Area service
            services.AddScoped<IChildEntityService<DMSAgenda, DMSAgendaReviewer>, ChildEntityService<DMSAgenda, DMSAgendaReviewer>>();
            services.AddScoped<IChildEntityService<DMSAgenda, DMSAgendaRelatedDisclosure>, ChildEntityService<DMSAgenda, DMSAgendaRelatedDisclosure>>();
            services.AddScoped<IChildEntityService<Disclosure, DMSCombined>, DMSDisclosureChildService<DMSCombined>>();

            services.AddScoped<IViewModelService<DMSFaqDoc>, ViewModelService<DMSFaqDoc>>();
            services.AddScoped<IEntityService<DMSFaqDoc>, AuxService<DMSFaqDoc>>();

            return services;
        }
    }
}
