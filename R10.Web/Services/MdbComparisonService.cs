using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LawPortal.Web.Services
{
    /// <summary>
    /// Result of comparing two MDB files.
    /// </summary>
    public class MdbComparisonResult
    {
        public bool IsPatent { get; set; }
        public Dictionary<string, TableDiff> TableDiffs { get; set; } = new();
    }

    public class TableDiff
    {
        public string TableName { get; set; } = "";
        public List<RowDiff> AddedRows { get; set; } = new();
        public List<RowDiff> DeletedRows { get; set; } = new();
        public List<RowDiff> ModifiedRows { get; set; } = new(); // Contains the NEW values with ChangedColumns set
    }

    public class RowDiff
    {
        public Dictionary<string, object?> Values { get; set; } = new();
        public Dictionary<string, object?>? OldValues { get; set; } // Only for modified rows
        public HashSet<string> ChangedColumns { get; set; } = new();
    }

    /// <summary>
    /// Compares two MDB files by reading them via the 32-bit R10.MdbReader helper.
    /// </summary>
    public class MdbComparisonService
    {
        private readonly string _mdbReaderPath;
        private readonly ILogger<MdbComparisonService> _logger;

        // Composite key definitions per table
        private static readonly Dictionary<string, string[]> TableKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            // Patent tables
            ["tblPatCountryLaw"] = new[] { "Country", "CaseType" },
            ["tblPatCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "BasedOn" },
            ["tblPatCountryExp"] = new[] { "Country", "CaseType", "Type", "BasedOn" },
            ["tblPatCountryExpDelete"] = new[] { "Country", "CaseType", "Type", "BasedOn" },
            ["tblPatDesCaseType"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
            ["tblPatArea"] = new[] { "Area" },
            ["tblPatAreaCountry"] = new[] { "Area", "Country" },
            ["tblPatAreaCountryDelete"] = new[] { "Area", "Country" },
            ["tblPatAreaDelete"] = new[] { "Area" },
            ["tblPatCaseType"] = new[] { "CaseType" },
            ["tblPatCountry"] = new[] { "Country" },
            ["tblPatCountryLaw_Ext"] = new[] { "Country", "CaseType" },
            ["tblPatCountryLawUpdate"] = new[] { "Country", "CaseType" },
            ["tblPatDesCaseType_Ext"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
            ["tblPatDesCaseTypeDelete"] = new[] { "CaseType", "DesCountry", "DesCaseType" },
            ["tblPatDesCaseTypeDelete_Ext"] = new[] { "CaseType", "DesCountry", "DesCaseType" },
            ["tblPatDesCaseTypeFields"] = new[] { "DesCaseType", "FromField" },
            ["tblPatDesCaseTypeFields_Ext"] = new[] { "DesCaseType", "FromField" },
            ["tblPatDesCaseTypeFieldsDelete"] = new[] { "DesCaseType", "FromField" },
            ["tblPatDesCaseTypeFieldsDelete_Ext"] = new[] { "DesCaseType", "FromField" },
            // Trademark tables
            ["tblTmkCountryLaw"] = new[] { "Country", "CaseType" },
            ["tblTmkCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "BasedOn" },
            ["tblTmkDesCaseType"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
            ["tblTmkArea"] = new[] { "Area" },
            ["tblTmkAreaCountry"] = new[] { "Area", "Country" },
            ["tblTmkAreaCountryDelete"] = new[] { "Area", "Country" },
            ["tblTmkAreaDelete"] = new[] { "Area" },
            ["tblTmkCaseType"] = new[] { "CaseType" },
            ["tblTmkCountry"] = new[] { "Country" },
            ["tblTmkCountryLawUpdate"] = new[] { "Country", "CaseType" },
            ["tblTmkDesCaseType_Ext"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
            ["tblTmkDesCaseTypeDelete"] = new[] { "CaseType", "DesCountry", "DesCaseType" },
            ["tblTmkDesCaseTypeDelete_Ext"] = new[] { "CaseType", "DesCountry", "DesCaseType" },
            ["tblTmkDesCaseTypeFields"] = new[] { "DesCaseType", "FromField" },
            ["tblTmkDesCaseTypeFields_Ext"] = new[] { "DesCaseType", "FromField" },
            ["tblTmkDesCaseTypeFieldsDelete"] = new[] { "DesCaseType", "FromField" },
            ["tblTmkDesCaseTypeFieldsDelete_Ext"] = new[] { "DesCaseType", "FromField" },
        };

        // Columns to ignore when comparing (audit fields, identity columns, computed fields)
        private static readonly HashSet<string> IgnoreColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "UserID", "CreatedBy", "UpdatedBy", "DateCreated", "LastUpdate", "tStamp", "Systems",
            "CDueId", "CExpId", "CountryLawID", "CPIPermanentID"
        };

        public MdbComparisonService(string webRootPath, ILogger<MdbComparisonService> logger)
        {
            _logger = logger;
            var webRoot = new DirectoryInfo(webRootPath);
            var solutionRoot = webRoot.Parent?.Parent;
            _mdbReaderPath = Path.Combine(solutionRoot?.FullName ?? "", "R10.MdbReader", "bin", "Debug", "net8.0", "R10.MdbReader.exe");
        }

        public async Task<MdbComparisonResult> CompareMdbFiles(string currentMdbPath, string oldMdbPath)
        {
            if (!File.Exists(_mdbReaderPath))
                throw new FileNotFoundException($"MDB Reader not found at: {_mdbReaderPath}. Please build the R10.MdbReader project.");

            _logger.LogInformation("MdbComparison: Reading {Current} vs {Old}", currentMdbPath, oldMdbPath);

            var psi = new ProcessStartInfo
            {
                FileName = _mdbReaderPath,
                Arguments = $"\"{currentMdbPath}\" \"{oldMdbPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) throw new Exception("Failed to start MDB Reader process.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(stderr))
                _logger.LogInformation("MdbReader output: {Output}", stderr);

            if (string.IsNullOrWhiteSpace(stdout))
                throw new Exception($"MDB Reader returned no data. Exit code: {process.ExitCode}. Error: {stderr}");

            // Parse JSON output
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Dictionary<string, JsonElement>>>>>(stdout);
            if (data == null) throw new Exception("Failed to parse MDB Reader JSON output.");

            var currentData = data.ContainsKey("file1") ? data["file1"] : new();
            var oldData = data.ContainsKey("file2") ? data["file2"] : new();

            // Determine if Patent or Trademark
            bool isPatent = currentData.Keys.Any(k => k.StartsWith("tblPat", StringComparison.OrdinalIgnoreCase));

            var result = new MdbComparisonResult { IsPatent = isPatent };

            // Compare each table
            var allTableNames = currentData.Keys.Union(oldData.Keys).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var tableName in allTableNames)
            {
                var currentRows = currentData.ContainsKey(tableName) ? currentData[tableName] : new();
                var oldRows = oldData.ContainsKey(tableName) ? oldData[tableName] : new();

                var keyColumns = TableKeys.ContainsKey(tableName) ? TableKeys[tableName] : new[] { "Id" };

                var diff = CompareTable(tableName, currentRows, oldRows, keyColumns);
                if (diff.AddedRows.Any() || diff.DeletedRows.Any() || diff.ModifiedRows.Any())
                    result.TableDiffs[tableName] = diff;
            }

            return result;
        }

        private TableDiff CompareTable(string tableName, List<Dictionary<string, JsonElement>> currentRows,
            List<Dictionary<string, JsonElement>> oldRows, string[] keyColumns)
        {
            var diff = new TableDiff { TableName = tableName };

            string GetKey(Dictionary<string, JsonElement> row)
            {
                return string.Join("|", keyColumns.Select(k =>
                    row.ContainsKey(k) ? row[k].ToString() : ""));
            }

            var currentByKey = new Dictionary<string, Dictionary<string, JsonElement>>();
            foreach (var row in currentRows)
            {
                var key = GetKey(row);
                currentByKey[key] = row;
            }

            var oldByKey = new Dictionary<string, Dictionary<string, JsonElement>>();
            foreach (var row in oldRows)
            {
                var key = GetKey(row);
                oldByKey[key] = row;
            }

            // Added rows (in current but not in old)
            foreach (var kvp in currentByKey)
            {
                if (!oldByKey.ContainsKey(kvp.Key))
                {
                    diff.AddedRows.Add(new RowDiff
                    {
                        Values = ConvertRow(kvp.Value),
                        ChangedColumns = new HashSet<string>(kvp.Value.Keys.Where(c => !IgnoreColumns.Contains(c)))
                    });
                }
            }

            // Deleted rows (in old but not in current)
            foreach (var kvp in oldByKey)
            {
                if (!currentByKey.ContainsKey(kvp.Key))
                {
                    diff.DeletedRows.Add(new RowDiff { Values = ConvertRow(kvp.Value) });
                }
            }

            // Modified rows (in both but with different values)
            foreach (var kvp in currentByKey)
            {
                if (oldByKey.ContainsKey(kvp.Key))
                {
                    var newRow = kvp.Value;
                    var oldRow = oldByKey[kvp.Key];
                    var changedCols = new HashSet<string>();

                    foreach (var col in newRow.Keys)
                    {
                        if (IgnoreColumns.Contains(col)) continue;
                        var newVal = NormalizeValue(newRow[col]);
                        var oldVal = oldRow.ContainsKey(col) ? NormalizeValue(oldRow[col]) : "";
                        if (newVal != oldVal)
                            changedCols.Add(col);
                    }

                    if (changedCols.Any())
                    {
                        diff.ModifiedRows.Add(new RowDiff
                        {
                            Values = ConvertRow(newRow),
                            OldValues = ConvertRow(oldRow),
                            ChangedColumns = changedCols
                        });
                    }
                }
            }

            return diff;
        }

        /// <summary>
        /// Normalize a JsonElement value for comparison — handles type differences,
        /// whitespace, date formatting, number precision, boolean casing.
        /// </summary>
        private string NormalizeValue(JsonElement el)
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return "";
                case JsonValueKind.True:
                    return "true";
                case JsonValueKind.False:
                    return "false";
                case JsonValueKind.Number:
                    // Normalize all numbers to the same format
                    if (el.TryGetInt64(out var l)) return l.ToString();
                    return el.GetDouble().ToString("G");
                case JsonValueKind.String:
                    var s = el.GetString() ?? "";
                    // Normalize dates: strip time component if it's midnight
                    if (DateTime.TryParse(s, out var dt))
                    {
                        if (dt.TimeOfDay == TimeSpan.Zero)
                            return dt.ToString("yyyy-MM-dd");
                        return dt.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    // Normalize whitespace: trim, collapse multiple spaces, normalize line endings
                    s = s.Trim().Replace("\r\n", "\n").Replace("\r", "\n");
                    while (s.Contains("  ")) s = s.Replace("  ", " ");
                    return s;
                default:
                    return el.ToString().Trim();
            }
        }

        private Dictionary<string, object?> ConvertRow(Dictionary<string, JsonElement> row)
        {
            var result = new Dictionary<string, object?>();
            foreach (var kvp in row)
            {
                result[kvp.Key] = kvp.Value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Number => kvp.Value.TryGetInt64(out var l) ? l : kvp.Value.GetDouble(),
                    _ => kvp.Value.ToString()
                };
            }
            return result;
        }
    }
}
