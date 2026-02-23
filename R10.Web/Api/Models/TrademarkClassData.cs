namespace R10.Web.Api.Models
{
    public class TrademarkClassData
    {
        public int TmkId { get; set; }

        public int TmkClassId { get; set; }

        public int ClassId { get; set; }

        public string? Class { get; set; }

        public string? ClassType { get; set; }

        public string? Goods { get; set; }

        public bool IsStandardGoods { get; set; }
    }
}
