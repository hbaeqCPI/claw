using System;
using System.Collections.Generic;
using System.Linq;

namespace LawPortal.Web.Helpers
{
    /// <summary>
    /// Provides the fixed, ordered list of systems used across all dropdowns and search screens.
    /// Systems are preset and cannot be added or deleted.
    /// </summary>
    public static class SystemsHelper
    {
        /// <summary>
        /// The canonical ordered list of system names.
        /// </summary>
        public static readonly IReadOnlyList<string> SystemNames = new[]
        {
            "R4",
            "PatR5-7",
            "PatR8-R10v2.1",
            "PatR10v2.2",
            "TmkR5-8",
            "TmkR9-10v2.2"
        };

        // R4 is shared across patent and trademark. Other systems live under one side
        // only and are identified by a Pat / Tmk prefix on the name.
        public static readonly IReadOnlyList<string> ForPatent = SystemNames
            .Where(s => s == "R4" || s.StartsWith("Pat", StringComparison.Ordinal))
            .ToList();

        public static readonly IReadOnlyList<string> ForTrademark = SystemNames
            .Where(s => s == "R4" || s.StartsWith("Tmk", StringComparison.Ordinal))
            .ToList();
    }
}
