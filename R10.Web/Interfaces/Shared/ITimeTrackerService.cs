using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Interfaces.Shared
{
    public interface ITimeTrackerService
    {
        Task<List<TimeTrackAttorney>> GetStartTimeTrackAttorneys(int id, string systemType);
        Task<bool> StartTimeTrack(int id, string systemType, string[] attorneyIds);
        Task<string?> StopTimeTrack();
    }
}
