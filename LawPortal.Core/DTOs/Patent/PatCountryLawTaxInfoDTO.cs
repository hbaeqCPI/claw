using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.DTOs
{
    public class PatCountryLawTaxInfoDTO
    {

        public int ExpTaxDateCount { get; set; }
        public int ExpTaxNoBaseDateCount { get; set; }
        public string? MissingBasedOnDateLawList { get; set; }
        public string? MessageType { get; set; }
        public string? Message { get; set; }
        public UserResponse RequireUserResponse { get; set; }
        public DateTime? ExpTaxDate { get; set; }
        public List<DateTime> ExpirationDates { get; set; }

        public PatCountryLawTaxInfoDTO()
        {
            ExpirationDates = new List<DateTime>();
        }

        public enum UserResponse
        {
            NotNeeded,
            JustOk,
            AcceptReject
        }

    }

    public class TaxDateMessageType
    {
        public const string Multiple = "Multiple";
        public const string Single = "Single";
        public const string WithPta = "WithPTA";
        public const string MissingBasedOnDate = "MissingBasedOnDate";
        public const string MultipleNoBaseDate = "MultipleNoBaseDate";
        public const string SingleNoBaseDate = "SingleNoBaseDate";
    }

    public enum TaxInfoUpdateType
    {
        Both,
        ExpireDate,
        TaxStartDate
    }
}


