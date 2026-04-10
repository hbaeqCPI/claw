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
            "97",
            "R5-7",
            "2000&Up",
            "R8&Up"
        };
    }
}
