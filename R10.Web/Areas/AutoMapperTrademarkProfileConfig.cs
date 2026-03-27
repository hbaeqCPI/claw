using AutoMapper;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Areas.Trademark.ViewModels.CountryLaw;
using R10.Core;
namespace R10.Web.Areas
{
    public class AutoMapperTrademarkProfileConfig : Profile
    {
        public AutoMapperTrademarkProfileConfig()
        {
            CreateMap<TmkActionType, ActionTypeSearchResultViewModel>();
            CreateMap<TmkActionType, ActionTypeViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.Ignore())
                .ForMember(vm => vm.ResponsibleCode, domain => domain.Ignore())
                .ForMember(vm => vm.ResponsibleName, domain => domain.Ignore());

            CreateMap<TmkCountryLaw, TmkCountryLawSearchViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.Ignore());

            CreateMap<TmkCountryLaw, TmkCountryLawDetailViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.Ignore())
                .ForMember(vm => vm.AgentCode, domain => domain.Ignore())
                .ForMember(vm => vm.AgentName, domain => domain.Ignore())
                .ForMember(vm => vm.CaseTypeDescription, domain => domain.Ignore())
                .ForMember(vm => vm.IsCPiAction, domain => domain.Ignore())
                .ForMember(sm => sm.TmkCountryDues, opt => opt.Ignore());

            CreateMap<TmkCountryDue, CountryLawRetroParam>();

            CreateMap<TmkDesCaseType, TmkDesCaseTypeViewModel>()
                .ForMember(vm => vm.DesCountryName, domain => domain.Ignore());

            CreateMap<TmkStandardGood, TmkStandardGoodListViewModel>()
                .ForMember(d => d.ClassDesc, o => o.MapFrom(s => s.Class + " - " + s.ClassType));

            CreateMap<TmkCountry, CountryLookupViewModel>()
                .ForMember(vm => vm.CountryID, opt => opt.Ignore());
            CreateMap<TmkAreaCountry, CountryAreaViewModel>()
                .ForMember(vm => vm.Area, domain => domain.MapFrom(d => d.Area != null ? d.Area.Area : null))
                .ForMember(vm => vm.AreaDescription, domain => domain.MapFrom(d => d.Area != null ? d.Area.Description : null))
                .ForMember(vm => vm.CountryLookup, domain => domain.Ignore());
            CreateMap<CountryAreaViewModel, TmkAreaCountry>()
                .ForMember(m => m.Area, opt => opt.Ignore());
        }
    }
}
