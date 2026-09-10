using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LawPortal.Web.Extensions
{
    /// <summary>
    /// Builds the keys the detail-page record navigator walks through.
    /// <para>
    /// The navigator was originally driven by <see cref="CPiDataSourceResult.Ids"/>, an
    /// int per row, which only works for screens whose Detail action takes an
    /// <c>int id</c>. Screens keyed on a composite of strings — Country Law
    /// (<c>country</c> + <c>caseType</c>), Des Case Type (four columns), the Area
    /// Country pairs — had nothing to navigate with.
    /// </para>
    /// <para>
    /// A key here is a query-string fragment naming the record, e.g.
    /// <c>Country=US&amp;CaseType=PAT&amp;Systems=R4</c>. The client appends it to the
    /// screen's Detail url, so the existing action signatures bind it straight from the
    /// query string with no changes: model binding is case-insensitive, so the property
    /// names used here only have to match the parameter names.
    /// </para>
    /// <para>
    /// Both sides of the comparison MUST produce byte-identical strings — the navigator
    /// finds its position with indexOf on the current record's key — so the grid rows and
    /// the detail page always build theirs through this class, in the same order.
    /// </para>
    /// </summary>
    public static class RecordNavigationKey
    {
        /// <summary>
        /// Builds one key from name/value pairs, in the order given.
        /// </summary>
        public static string Build(params (string Name, object Value)[] parts)
        {
            if (parts == null || parts.Length == 0)
                return "";

            return string.Join("&", parts.Select(p =>
                $"{p.Name}={Uri.EscapeDataString(p.Value?.ToString() ?? "")}"));
        }

        /// <summary>
        /// Builds one key per row by reading named properties off each row.
        /// <para>
        /// Each spec is either <c>"Property"</c>, or <c>"queryName:Property"</c> where the
        /// two differ — which is common, because the Detail actions name their parameters
        /// differently from the grid columns (<c>id</c> holds Area, <c>areaCode</c> holds
        /// Area, <c>cExpId</c> holds CExpId). The query name is what the action binds
        /// from, so it must match the parameter name; take both from the screen's own
        /// search-grid row link, which is the authority on how that screen opens a record.
        /// </para>
        /// </summary>
        public static string[] BuildAll<T>(IEnumerable<T> rows, params string[] specs)
        {
            if (rows == null || specs == null || specs.Length == 0)
                return Array.Empty<string>();

            // Resolve the query names and property accessors once rather than per row.
            var parts = specs
                .Select(spec =>
                {
                    var split = spec.Split(':');
                    var queryName = split[0];
                    var propertyName = split.Length > 1 ? split[1] : split[0];

                    return (
                        Name: queryName,
                        Accessor: typeof(T).GetProperty(propertyName,
                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance));
                })
                .ToArray();

            if (parts.Any(p => p.Accessor == null))
                return Array.Empty<string>();

            return rows
                .Select(row => Build(parts
                    .Select(p => (p.Name, p.Accessor.GetValue(row)))
                    .ToArray()))
                .ToArray();
        }

        /// <summary>
        /// The property names out of a spec list, for selecting just those columns.
        /// </summary>
        public static string[] PropertyNames(params string[] specs)
        {
            if (specs == null)
                return Array.Empty<string>();

            return specs
                .Select(spec =>
                {
                    var split = spec.Split(':');
                    return split.Length > 1 ? split[1] : split[0];
                })
                .ToArray();
        }
    }
}
