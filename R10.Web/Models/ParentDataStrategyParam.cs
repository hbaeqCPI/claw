namespace R10.Web.Models
{
    /// <summary>
    /// Parameter for resolving parent record data used by Quick Email and DocuSign.
    /// </summary>
    public class ParentDataStrategyParam
    {
        public int Id { get; set; }
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDocLibraryFolder { get; set; }
        public string? SharePointRecKey { get; set; }
    }
}
