// Stub interfaces and no-op implementations - originals removed during debloat.
// These exist only to satisfy DI constructor injection in surviving files
// that still reference them (Documents, DocumentVerification, CPIGoogle, etc.)
// These can be safely deleted once those consuming files are also removed.

namespace LawPortal.Web.Interfaces
{
    public interface IInventionViewModelService { }
    public interface ICountryApplicationViewModelService { }
    public interface IPatActionDueViewModelService { }
    public interface IPatActionDueInvViewModelService { }
    public interface IPatCostTrackingViewModelService { }
    public interface IPatCostTrackingInvViewModelService { }
    public interface IPatImageInvViewModelService { }
    public interface IPatImageAppViewModelService { }
    public interface IPatImageActViewModelService { }
    public interface IPatImageActInvViewModelService { }
    public interface IPatImageCostViewModelService { }
    public interface IPatImageCostInvViewModelService { }
    public interface IPatInventorRemunerationService { }
    public interface IPatInventorFRRemunerationService { }
    public interface IPatInventorAppAwardUpdateService { }
    public interface IEPOService { }
    public interface ITmkTrademarkViewModelService { }
    public interface ITmkActionDueViewModelService { }
    public interface ITmkCostTrackingViewModelService { }
    public interface ITmkConflictViewModelService { }
    public interface ITmkImageViewModelService { }
    public interface ITmkImageCostViewModelService { }
    public interface ITmkImageActViewModelService { }

    // No-op implementations for DI registration
    internal class NoOpInventionViewModelService : IInventionViewModelService { }
    internal class NoOpCountryApplicationViewModelService : ICountryApplicationViewModelService { }
    internal class NoOpPatActionDueViewModelService : IPatActionDueViewModelService { }
    internal class NoOpPatActionDueInvViewModelService : IPatActionDueInvViewModelService { }
    internal class NoOpPatCostTrackingViewModelService : IPatCostTrackingViewModelService { }
    internal class NoOpPatCostTrackingInvViewModelService : IPatCostTrackingInvViewModelService { }
    internal class NoOpPatImageInvViewModelService : IPatImageInvViewModelService { }
    internal class NoOpPatImageAppViewModelService : IPatImageAppViewModelService { }
    internal class NoOpPatImageActViewModelService : IPatImageActViewModelService { }
    internal class NoOpPatImageActInvViewModelService : IPatImageActInvViewModelService { }
    internal class NoOpPatImageCostViewModelService : IPatImageCostViewModelService { }
    internal class NoOpPatImageCostInvViewModelService : IPatImageCostInvViewModelService { }
    internal class NoOpPatInventorRemunerationService : IPatInventorRemunerationService { }
    internal class NoOpPatInventorFRRemunerationService : IPatInventorFRRemunerationService { }
    internal class NoOpPatInventorAppAwardUpdateService : IPatInventorAppAwardUpdateService { }
    internal class NoOpEPOService : IEPOService { }
    internal class NoOpTmkTrademarkViewModelService : ITmkTrademarkViewModelService { }
    internal class NoOpTmkActionDueViewModelService : ITmkActionDueViewModelService { }
    internal class NoOpTmkCostTrackingViewModelService : ITmkCostTrackingViewModelService { }
    internal class NoOpTmkConflictViewModelService : ITmkConflictViewModelService { }
    internal class NoOpTmkImageViewModelService : ITmkImageViewModelService { }
    internal class NoOpTmkImageCostViewModelService : ITmkImageCostViewModelService { }
    internal class NoOpTmkImageActViewModelService : ITmkImageActViewModelService { }
}
