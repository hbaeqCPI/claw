using AutoMapper;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Patent.ViewModels.CountryLaw;
using R10.Web.Areas.Shared.ViewModels;
using R10.Core;
using R10.Core.Helpers;
namespace R10.Web.Areas
{
    public class AutoMapperPatentProfileConfig : Profile
    {
        public AutoMapperPatentProfileConfig()
        {
            CreateMap<PatActionType, ActionTypeSearchResultViewModel>();
            CreateMap<PatActionType, ActionTypeViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(d => d.PatCountry.CountryName))
                .ForMember(vm => vm.ResponsibleCode, domain => domain.MapFrom(d => d.Responsible != null ? d.Responsible.AttorneyCode : null))
                .ForMember(vm => vm.ResponsibleName, domain => domain.MapFrom(d => d.Responsible != null ? d.Responsible.AttorneyName : null));

            CreateMap<PatActionType, ActionDueRetroParam>()
                .ForMember(vm => vm.ActionTypeID, domain => domain.MapFrom(d => d.ActionTypeID));
            CreateMap<PatActionParameter, ActionDueRetroParam>()
                .ForMember(vm => vm.ActionTypeID, domain => domain.MapFrom(d => d.ActionTypeID))
                .ForMember(vm => vm.ActParamId, domain => domain.MapFrom(d => d.ActParamId))
                .ForMember(vm => vm.ActionDue, domain => domain.MapFrom(d => d.ActionDue))
                .ForMember(vm => vm.ActionType, domain => domain.MapFrom(d => d.ActionType != null ? d.ActionType.ActionType : null))
                .ForMember(vm => vm.Country, domain => domain.MapFrom(d => d.ActionType != null ? d.ActionType.Country : null));

            CreateMap<PatCountryLaw, PatCountryLawSearchViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(d => d.PatCountry.CountryName));

            CreateMap<PatCountryLaw, PatCountryLawDetailViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(d => d.PatCountry.CountryName))
                .ForMember(vm => vm.AgentCode, domain => domain.MapFrom(d => d.Agent != null ? d.Agent.AgentCode : null))
                .ForMember(vm => vm.AgentName, domain => domain.MapFrom(d => d.Agent != null ? d.Agent.AgentName : null))
                .ForMember(vm => vm.CaseTypeDescription, domain => domain.MapFrom(d => d.PatCaseType != null ? d.PatCaseType.Description : null))
                .ForMember(vm => vm.IsCPiAction, domain => domain.MapFrom(d => d.PatCountryDues != null && d.PatCountryDues.Any(cd => cd.CPIAction)))
                .ForMember(vm => vm.PatCaseType, opt => opt.Ignore())
                .ForMember(vm => vm.Agent, opt => opt.Ignore())
                .ForMember(vm => vm.PatCountry, opt => opt.Ignore())
                .ForMember(sm => sm.PatCountryDues, opt => opt.Ignore());

            CreateMap<PatCountryDue, CountryLawRetroParam>();

            CreateMap<PatDesCaseType, PatDesCaseTypeViewModel>()
                .ForMember(vm => vm.DesCountryName, domain => domain.MapFrom(d => d.ChildCountry != null ? d.ChildCountry.CountryName : null))
                .ForMember(vm => vm.GenApp, domain => domain.MapFrom(d => d.GenApp ?? false))
                .ForMember(vm => vm.PatCountryLaw, opt => opt.Ignore());

            CreateMap<PatAreaCountry, CountryAreaViewModel>()
                .ForMember(vm => vm.Area, domain => domain.MapFrom(d => d.Area != null ? d.Area.Area : null))
                .ForMember(vm => vm.AreaDescription, domain => domain.MapFrom(d => d.Area != null ? d.Area.Description : null));
            CreateMap<CountryAreaViewModel, PatAreaCountry>()
                .ForMember(m => m.Area, opt => opt.Ignore())
                .ForMember(m => m.AreaCountry, opt => opt.Ignore());
        }
    }
}
