using System.Collections.Generic;

namespace R10.Web.Helpers
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
    }
}
