using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.Clearance;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.Clearance;
//using R10.Web.Areas.Clearance.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class ClearanceServiceCollectionExtensions
    {
        public static IServiceCollection AddClearance(this IServiceCollection services)
        {
            //clearance
            //services.AddScoped<ITmcClearanceViewModelService, TmcClearanceViewModelService>(); // Removed: Web Area service
            services.AddScoped<ITmcClearanceService, TmcClearanceService>();

            //questions
            services.AddScoped<IChildEntityService<TmcClearance, TmcQuestion>, TmcClearanceChildService<TmcQuestion>>();

            //keyword
            services.AddScoped<IChildEntityService<TmcClearance, TmcKeyword>, TmcClearanceChildService<TmcKeyword>>();

            //images
            //services.AddScoped<ITmcImageViewModelService, TmcImageViewModelService>(); // Removed: Web Area service

            //related trademark
            services.AddScoped<IChildEntityService<TmcClearance, TmcRelatedTrademark>, TmcClearanceChildService<TmcRelatedTrademark>>();

            //list
            services.AddScoped<IMultipleEntityService<TmcClearance, TmcList>, TmcListService>();

            //requested marks/terms
            services.AddScoped<IChildEntityService<TmcClearance, TmcMark>, TmcClearanceChildService<TmcMark>>();

            //workflow
            services.AddScoped<ITmcWorkflowService, TmcWorkflowService>();
            services.AddScoped<IAsyncRepository<TmcWorkflow>, EFRepository<TmcWorkflow>>();
            services.AddScoped<IViewModelService<TmcWorkflow>, ViewModelService<TmcWorkflow>>();

            // questionnaire
            services.AddScoped<ITmcQuestionnaireService, TmcQuestionnaireService>();
            services.AddScoped<IAsyncRepository<TmcQuestionGroup>, EFRepository<TmcQuestionGroup>>();
            services.AddScoped<IViewModelService<TmcQuestionGroup>, ViewModelService<TmcQuestionGroup>>();

            //auxiliary
            services.AddScoped<IViewModelService<TmcClearanceStatus>, ViewModelService<TmcClearanceStatus>>();
            services.AddScoped<IEntityService<TmcClearanceStatus>, AuxService<TmcClearanceStatus>>();

            //settings
            services.AddScoped<ISystemSettings<TmcSetting>, SystemSettings<TmcSetting>>();

            //review
            //services.AddScoped<ITmcReviewViewModelService, TmcReviewViewModelService>(); // Removed: Web Area service

            //discussions
            services.AddScoped<IChildEntityService<TmcClearance, TmcDiscussion>, TmcClearanceChildService<TmcDiscussion>>();
            services.AddScoped<IParentEntityService<TmcDiscussion, TmcDiscussionReply>, ParentEntityService<TmcDiscussion, TmcDiscussionReply>>();

            return services;
        }
    }


}
