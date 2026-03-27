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
                .ForMember(vm => vm.CountryName, domain => domain.Ignore())
                .ForMember(vm => vm.ResponsibleCode, domain => domain.Ignore())
                .ForMember(vm => vm.ResponsibleName, domain => domain.Ignore());

            // ActionDueRetroParam mappings removed during debloat

            CreateMap<PatCountryLaw, PatCountryLawSearchViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.Ignore());

            CreateMap<PatCountryLaw, PatCountryLawDetailViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.Ignore())
                .ForMember(vm => vm.AgentCode, domain => domain.Ignore())
                .ForMember(vm => vm.AgentName, domain => domain.Ignore())
                .ForMember(vm => vm.CaseTypeDescription, domain => domain.Ignore())
                .ForMember(vm => vm.IsCPiAction, domain => domain.Ignore())
                .ForMember(sm => sm.PatCountryDues, opt => opt.Ignore());

            CreateMap<PatCountryDue, CountryLawRetroParam>();

            CreateMap<PatDesCaseType, PatDesCaseTypeViewModel>()
                .ForMember(vm => vm.DesCountryName, domain => domain.Ignore());

            CreateMap<PatCountry, CountryLookupViewModel>()
                .ForMember(vm => vm.CountryID, opt => opt.Ignore());
            CreateMap<PatAreaCountry, CountryAreaViewModel>()
                .ForMember(vm => vm.Area, domain => domain.MapFrom(d => d.Area != null ? d.Area.Area : null))
                .ForMember(vm => vm.AreaDescription, domain => domain.MapFrom(d => d.Area != null ? d.Area.Description : null))
                .ForMember(vm => vm.CountryLookup, domain => domain.Ignore());
            CreateMap<CountryAreaViewModel, PatAreaCountry>()
                .ForMember(m => m.Area, opt => opt.Ignore());
        }
    }
}
