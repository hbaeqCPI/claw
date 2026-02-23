namespace R10.Web.Api.Models
{
    public class InventionData
    {
        public int InvId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Title { get; set; }
        public string? DisclosureStatus { get; set; }
        public DateTime? DisclosureDate { get; set; }


        //ENTITIES
        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }
        public string? ClientRef { get; set; }
        public string? Attorney1Code { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney1Email { get; set; }
        public string? Attorney2Code { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney2Email { get; set; }
        public string? Attorney3Code { get; set; }
        public string? Attorney3Name { get; set; }
        public string? Attorney3Email { get; set; }
        public string? Attorney4Code { get; set; }
        public string? Attorney4Name { get; set; }
        public string? Attorney4Email { get; set; }
        public string? Attorney5Code { get; set; }
        public string? Attorney5Name { get; set; }
        public string? Attorney5Email { get; set; }
        public List<InventorData>? Inventors { get; set; }
        public List<OwnerData>? Owners { get; set; }


        public string? Remarks { get; set; }

        public string? RespOffice { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
