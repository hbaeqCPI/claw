namespace R10.Web.Areas.Admin.ViewModels
{
    public class UserUpdateAction
    {
        public string UserId { get; set; }
        public int Action { get; set; }
    }

    public enum UpdateAction
    {
        Enable,
        Approve,
        Reject,
        Disable,
        Reactivate,
        Unlock,
        Resend
    }
}
