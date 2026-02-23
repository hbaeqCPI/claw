using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.PatClearance;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.PatClearance;
//using R10.Web.Areas.PatClearance.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class PatentClearanceServiceCollectionExtensions
    {
        public static IServiceCollection AddPatentClearance(this IServiceCollection services)
        {
            //clearance
            //services.AddScoped<IPacClearanceViewModelService, PacClearanceViewModelService>(); // Removed: Web Area service
            services.AddScoped<IPacClearanceService, PacClearanceService>();

            services.AddScoped<IPacInventorService, PacInventorService>();
            services.AddScoped<IEntityService<PacInventor>, AuxService<PacInventor>>();

            services.AddScoped<IChildEntityService<PacClearance, PacKeyword>, PacClearanceChildService<PacKeyword>>();

            //questions
            services.AddScoped<IChildEntityService<PacClearance, PacQuestion>, PacClearanceChildService<PacQuestion>>();

            //images
            //services.AddScoped<IPacImageViewModelService, PacImageViewModelService>(); // Removed: Web Area service

            //workflow
            services.AddScoped<IPacWorkflowService, PacWorkflowService>();
            services.AddScoped<IAsyncRepository<PacWorkflow>, EFRepository<PacWorkflow>>();
            services.AddScoped<IViewModelService<PacWorkflow>, ViewModelService<PacWorkflow>>();

            // questionnaire
            services.AddScoped<IPacQuestionnaireService, PacQuestionnaireService>();
            services.AddScoped<IAsyncRepository<PacQuestionGroup>, EFRepository<PacQuestionGroup>>();
            services.AddScoped<IViewModelService<PacQuestionGroup>, ViewModelService<PacQuestionGroup>>();

            //auxiliary
            services.AddScoped<IViewModelService<PacClearanceStatus>, ViewModelService<PacClearanceStatus>>();
            services.AddScoped<IEntityService<PacClearanceStatus>, AuxService<PacClearanceStatus>>();

            //settings
            services.AddScoped<ISystemSettings<PacSetting>, SystemSettings<PacSetting>>();

            //review
            //services.AddScoped<IPacReviewViewModelService, PacReviewViewModelService>(); // Removed: Web Area service

            //discussions
            services.AddScoped<IChildEntityService<PacClearance, PacDiscussion>, PacClearanceChildService<PacDiscussion>>();
            services.AddScoped<IParentEntityService<PacDiscussion, PacDiscussionReply>, ParentEntityService<PacDiscussion, PacDiscussionReply>>();

            return services;
        }
    }


}
