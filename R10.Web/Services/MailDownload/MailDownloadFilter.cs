namespace R10.Web.Services.MailDownload
{
    public class MailDownloadFilter
    {
        public string Name { get; set; }
        public int Length { get; set; }
        public bool ValueHasNoSpace { get; set; }
        public List<string> Patterns { get; set; }
    }
}
