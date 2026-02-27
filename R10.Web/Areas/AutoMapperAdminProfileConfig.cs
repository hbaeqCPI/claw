using AutoMapper;
using R10.Core.Entities;
using R10.Core.Entities.MailDownload;
using R10.Core.Entities.Shared;
using R10.Core.Identity;
using R10.Infrastructure.Data;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Areas
{
    public class AutoMapperAdminProfileConfig : Profile
    {
        public AutoMapperAdminProfileConfig()
        {
            string? userId = null;
            int? loginInactiveDays = null;

            CreateMap<CPiUser, UserListViewModel>()
                .ForMember(vm => vm.FullName, domain => domain.MapFrom(d => string.Concat(d.FirstName, " ", d.LastName)))
                .ForMember(vm => vm.IsSuper, domain => domain.MapFrom(d => d.UserType == CPiUserType.SuperAdministrator))
                .ForMember(vm => vm.Inactive, domain => domain.MapFrom(d => d.IsEnabled && d.LastLoginDate != null && loginInactiveDays > 0 && (DateTime.Now - d.LastLoginDate).Value.Days >= loginInactiveDays))
                .ForMember(vm => vm.StatusDisplay, domain => domain.MapFrom(d => d.Status.GetDisplayName()))
                .ForMember(vm => vm.UserTypeDisplay, domain => domain.MapFrom(d => d.UserType.GetDisplayName()))
                .ForMember(vm => vm.IsSelf, domain => domain.MapFrom(d => d.Id == (userId ?? "")));
            CreateMap<CPiUser, UserDetailViewModel>()
                .ForMember(vm => vm.FullName, domain => domain.MapFrom(d => string.Concat(d.FirstName, " ", d.LastName)))
                .ForMember(vm => vm.Inactive, domain => domain.MapFrom(d => d.IsEnabled && d.LastLoginDate != null && loginInactiveDays > 0 && (DateTime.Now - d.LastLoginDate).Value.Days >= loginInactiveDays))
                .ForMember(vm => vm.LockoutEnd, domain => domain.MapFrom(d => d.LockoutEnd == null ? null : (DateTime?)d.LockoutEnd.Value.LocalDateTime))
                .ForMember(vm => vm.StatusDisplay, domain => domain.MapFrom(d => d.Status.GetDisplayName()))
                .ForMember(vm => vm.UserTypeDisplay, domain => domain.MapFrom(d => d.UserType.GetDisplayName()));
            CreateMap<CPiMenuPage, MenuPageListViewModel>()
                .ForMember(vm => vm.Area, domain => domain.MapFrom(d => SqlHelper.JsonValue(d.RouteOptions, "$.area")));
            CreateMap<CPiMenuPage, MenuPageDetailViewModel>();
            CreateMap<CPiGroup, GroupListViewModel>();
            CreateMap<CPiGroup, GroupDetailViewModel>();
            CreateMap<CPiUserGroup, GroupUsersViewModel>()
                .ForMember(vm => vm.Email, domain => domain.MapFrom(d => d.CPiUser.Email))
                .ForMember(vm => vm.User, domain => domain.MapFrom(d => new PickListViewModel()
                {
                    Id = d.UserId,
                    Name = string.Concat(d.CPiUser.FirstName, " ", d.CPiUser.LastName)
                }));
            CreateMap<GroupUsersViewModel, CPiUserGroup>()
                .ForMember(d => d.UserId, option => option.MapFrom(vm => vm.User.Id));
            CreateMap<CPiUser, PickListViewModel>()
                .ForMember(vm => vm.Name, domain => domain.MapFrom(d => string.Concat(d.FirstName, " ", d.LastName)));
            CreateMap<CPiUserGroup, UserGroupsViewModel>()
                .ForMember(vm => vm.Group, domain => domain.MapFrom(d => new PickListViewModel()
                {
                    Id = d.GroupId.ToString(),
                    Name = d.CPiGroup.Name
                }));
            CreateMap<UserGroupsViewModel, CPiUserGroup>()
                .ForMember(d => d.GroupId, option => option.MapFrom(vm => int.Parse(vm.Group.Id)));
            CreateMap<MailDownloadDataMap, MailDataMapListViewModel>()
                .ForMember(vm => vm.AttributeName, domain => domain.MapFrom(d => d.Attribute.Name));
            CreateMap<MailDownloadDataMap, MailDataMapDetailViewModel>()
                .ForMember(vm => vm.AttributeName, domain => domain.MapFrom(d => d.Attribute.Name));

            CreateMap<TradeSecretRequest, TradeSecretRequestListViewModel>()
                .ForMember(vm => vm.Email, domain => domain.MapFrom(d => d.CPiUser.Email));

            CreateMap<TradeSecretRequest, TradeSecretRequestDetailViewModel>()
                .ForMember(vm => vm.Email, domain => domain.MapFrom(d => d.CPiUser.Email));
            //.ForMember(vm => vm.CPiUser, opt => opt.Ignore());

            CreateMap<ScheduledTask, ScheduledTaskListViewModel>();
            CreateMap<ScheduledTask, ScheduledTaskDetailViewModel>()
                .ForMember(vm => vm.Password, opt => opt.Ignore())
                .ForMember(vm => vm.RequestContentType, domain => domain.MapFrom(d => d.RequestContentType ?? ScheduledTaskRequestContentType.StringContent))
                .ForMember(vm => vm.RequestMethod, domain => domain.MapFrom(d => d.RequestMethod ?? "GET"))
                .ForMember(vm => vm.GrantType, domain => domain.MapFrom(d => d.GrantType ?? "password"));
            CreateMap<ScheduledTask, ScheduledSystemStatusListViewModel>();
            CreateMap<ScheduledTask, ScheduledSystemStatusDetailViewModel>()
                .ForMember(vm => vm.Password, opt => opt.Ignore());
        }
    }
}
