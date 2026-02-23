using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Entities
{
    public class SystemType
    {
        //CPiSystem IDs
        public const string Patent = "Patent";
        public const string Trademark = "Trademark";
        public const string GeneralMatter = "GeneralMatter";
        public const string AMS = "AMS";
        public const string DMS = "DMS";
        public const string IDS = "IDS";
        public const string Shared = "Shared";
        public const string SearchRequest = "SearchRequest";
        public const string RMS = "RMS";
        public const string PatClearance = "PatClearance";
        public const string ForeignFiling = "ForeignFiling";

        //List of systems that use ContactPerson user type
        public static readonly string[] HasContactPersonUser = { AMS, DMS, SearchRequest, PatClearance, RMS, ForeignFiling };

        //List of systems that use Attorney user type
        public static readonly string[] HasAttorneyUser = { AMS, Patent, Trademark, GeneralMatter };

        //List of systems that use Quick Email
        public static readonly string[] HasQuickEmail = { Patent, PatClearance, AMS, DMS, Trademark, SearchRequest, GeneralMatter }; //PEADTCG

        //List of systems that use Workflow
        public static readonly string[] HasWorkflow = { Patent, PatClearance, DMS, Trademark, SearchRequest, GeneralMatter }; //PEDTCG

        //List of systems that use Action Delegation
        public static readonly string[] HasActionDelegation = { Patent, Trademark, GeneralMatter };

        //List of systems that has Dashboard widgets
        public static readonly string[] HasDashboardWidgets = { Patent, PatClearance, AMS, DMS, Trademark, SearchRequest, GeneralMatter };

        /// <summary>
        /// One character system type
        /// </summary>
        public string? TypeId { get; set; }

        /// <summary>
        /// System name
        /// CPiSystem.Id
        /// </summary>
        public string? SystemId { get; set; }
    }
}
