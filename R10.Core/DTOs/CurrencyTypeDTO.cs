namespace R10.Core.DTOs
{
    public class CurrencyExRateUpdateDTO
    {
        public string? CurrencyType { get; set; }
        public string? Description { get; set; }
        public string? Symbol { get; set; }
        public bool InCirculation { get; set; }

        // US Dollar
        public double? USD_ExRate { get; set; }
        public DateTime? USD_ExRateLastUpdate { get; set; }

        // Euro
        public double? EUR_ExRate { get; set; }
        public DateTime? EUR_ExRateLastUpdate { get; set; }

        // British Pound
        public double? GBP_ExRate { get; set; }
        public DateTime? GBP_ExRateLastUpdate { get; set; }

        // Danish Krone
        public double? DKK_ExRate { get; set; }
        public DateTime? DKK_ExRateLastUpdate { get; set; }
    }
}
