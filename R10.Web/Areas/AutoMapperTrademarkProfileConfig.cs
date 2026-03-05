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
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(d => d.TmkCountry.CountryName))
                .ForMember(vm => vm.ResponsibleCode, domain => domain.Ignore())
                .ForMember(vm => vm.ResponsibleName, domain => domain.Ignore());

            CreateMap<TmkCountryLaw, TmkCountryLawSearchViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(d => d.TmkCountry.CountryName));

            CreateMap<TmkCountryLaw, TmkCountryLawDetailViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(d => d.TmkCountry.CountryName))
                .ForMember(vm => vm.AgentCode, domain => domain.Ignore())
                .ForMember(vm => vm.AgentName, domain => domain.Ignore())
                .ForMember(vm => vm.CaseTypeDescription, domain => domain.MapFrom(d => d.TmkCaseType != null ? d.TmkCaseType.Description : null))
                .ForMember(vm => vm.IsCPiAction, domain => domain.MapFrom(d => d.TmkCountryDues != null && d.TmkCountryDues.Any(cd => cd.CPIAction)))
                .ForMember(vm => vm.TmkCaseType, opt => opt.Ignore())
                .ForMember(vm => vm.TmkCountry, opt => opt.Ignore())
                .ForMember(sm => sm.TmkCountryDues, opt => opt.Ignore());

            CreateMap<TmkCountryDue, CountryLawRetroParam>();

            CreateMap<TmkDesCaseType, TmkDesCaseTypeViewModel>()
                .ForMember(vm => vm.DesCountryName, domain => domain.MapFrom(d => d.ChildCountry != null ? d.ChildCountry.CountryName : null))
                .ForMember(vm => vm.GenApp, domain => domain.MapFrom(d => d.GenApp ?? false));

            CreateMap<TmkAreaCountry, CountryAreaViewModel>()
                .ForMember(vm => vm.Area, domain => domain.MapFrom(d => d.Area != null ? d.Area.Area : null))
                .ForMember(vm => vm.AreaDescription, domain => domain.MapFrom(d => d.Area != null ? d.Area.Description : null));
            CreateMap<CountryAreaViewModel, TmkAreaCountry>()
                .ForMember(m => m.Area, opt => opt.Ignore())
                .ForMember(m => m.AreaCountry, opt => opt.Ignore());
        }
    }
}
