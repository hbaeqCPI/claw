namespace LawPortal.Web.Services
{
    /// <summary>
    /// Credentials/config for the MOVEit Transfer (MFT) server that the Deploy
    /// screen's Push button uploads to. Populated from the "MoveItMft" section
    /// of appsettings.json so the secrets stay out of source/UI. Files are uploaded
    /// into the login's default folder, so no folder path is configured here.
    /// </summary>
    public class MoveItMftSettings
    {
        // e.g. https://mft.computerpackages.com  (no trailing slash, no /api/v1)
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
