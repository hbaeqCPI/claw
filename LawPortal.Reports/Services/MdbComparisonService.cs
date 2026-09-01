using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LawPortal.Reports.Services
{
    /// <summary>
    /// Result of comparing two MDB files.
    /// </summary>
    public class MdbComparisonResult
    {
        public bool IsPatent { get; set; }
        public Dictionary<string, TableDiff> TableDiffs { get; set; } = new();

        // Raw row counts per table for each source file, keyed by table name.
        // Used to detect an incomplete source MDB (e.g. a whole table present in
        // one file but empty in the other) so the report can warn instead of
        // silently emitting a diff full of phantom adds/deletes.
        public Dictionary<string, int> CurrentRowCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> OldRowCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        // Raw rows of the OLD (comparison) MDB file, per table, as plain values.
        // Lets a caller re-diff a specific table against a different "current"
        // source (e.g. the live DB tables for ActionType/ActionParameter) while
        // keeping the old MDB as the baseline.
        public Dictionary<string, List<Dictionary<string, object?>>> OldFileRows { get; set; }
            = new(StringComparer.OrdinalIgnoreCase);
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
    /// Compares two MDB files by reading them via the 32-bit LawPortal.MdbReader helper.
    /// </summary>
    public class MdbComparisonService
    {
        private readonly string _mdbReaderPath;
        private readonly ILogger<MdbComparisonService> _logger;

        // Composite key definitions per table
        private static readonly Dictionary<string, string[]> TableKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            // ActionType tables — key on (ActionType, Country) only. CDueId is an
            // identity FK into CountryDue that regenerates per MDB export, so including
            // it in the key makes unchanged ActionType rows appear as deleted+added.
            // The Manual Updates section of the report diffs these tables.
            ["tblPatActionType"] = new[] { "ActionType", "Country" },
            ["tblTmkActionType"] = new[] { "ActionType", "Country" },
            // ActionParameter tables — per-ActionType templates. The ActionTypeID FK
            // is stable across MDB exports from the same source database, so we key
            // on it along with the business values (ActionDue + Yr/Mo/Dy).
            ["tblPatActionParameter"] = new[] { "ActionTypeID", "ActionDue", "Yr", "Mo", "Dy" },
            ["tblTmkActionParameter"] = new[] { "ActionTypeID", "ActionDue", "Yr", "Mo", "Dy" },
            // Standard Goods — trademark only. Natural key is (Class, ClassType);
            // ClassId is an identity that changes per MDB export.
            ["tblTmkStandardGood"] = new[] { "Class", "ClassType" },
            // Patent tables
            ["tblPatCountryLaw"] = new[] { "Country", "CaseType" },
            // CountryDue can hold multiple rows for the same (Country, CaseType,
            // ActionType, ActionDue, BasedOn) when a deadline is being phased
            // in/out — old rule effective until date X, new rule effective from
            // date Y. Include EffStartDate/EffEndDate in the key so both rows
            // survive the diff instead of collapsing into one. Same logic for
            // CountryExp.
            ["tblPatCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "BasedOn", "EffStartDate", "EffEndDate" },
            ["tblPatCountryExp"] = new[] { "Country", "CaseType", "Type", "BasedOn", "EffStartDate", "EffEndDate" },
            ["tblPatCountryExpDelete"] = new[] { "Country", "CaseType", "Type", "BasedOn", "EffStartDate", "EffEndDate" },
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
            ["tblTmkCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "BasedOn", "EffStartDate", "EffEndDate" },
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
            "CDueId", "CExpId", "CountryLawID", "CPIPermanentID",
            // ActionType identity column and a FK link-table-ish field that varies by release
            "ActionTypeID", "ResponsibleID",
            // ActionParameter identity column — reassigned when the table is reloaded,
            // so it differs between the live table and a quarter snapshot.
            "ActParamId",
            // StandardGood identity column
            "ClassId",
            // Present only on hist_* snapshot tables, never on the live table.
            "SnapshotYear", "SnapshotQuarter"
        };

        public MdbComparisonService(string webRootPath, ILogger<MdbComparisonService> logger)
        {
            _logger = logger;
            _mdbReaderPath = ResolveMdbExePath(webRootPath);
        }

        // Resolve LawPortal.Mdb.exe location.
        // Production/staging: <deployFolder>\mdbservice\LawPortal.Mdb.exe
        //   (deployFolder = parent of wwwroot static-files folder, where LawPortal.Web.dll lives)
        // Dev fallback:     <solutionRoot>\LawPortal.Mdb\bin\{Debug|Release}\net8.0\LawPortal.Mdb.exe
        private static string ResolveMdbExePath(string webRootPath)
        {
            var deployFolder = new DirectoryInfo(webRootPath).Parent?.FullName ?? webRootPath;
            var deployed = Path.Combine(deployFolder, "mdbservice", "LawPortal.Mdb.exe");
            if (File.Exists(deployed)) return deployed;

            var solutionRoot = new DirectoryInfo(webRootPath).Parent?.Parent?.FullName;
            if (solutionRoot != null)
            {
                foreach (var cfg in new[] { "Debug", "Release" })
                {
                    var dev = Path.Combine(solutionRoot, "LawPortal.Mdb", "bin", cfg, "net8.0", "LawPortal.Mdb.exe");
                    if (File.Exists(dev)) return dev;
                }
            }

            return deployed;
        }

        public async Task<MdbComparisonResult> CompareMdbFiles(string currentMdbPath, string oldMdbPath)
        {
            if (!File.Exists(_mdbReaderPath))
                throw new FileNotFoundException($"LawPortal.Mdb.exe not found at: {_mdbReaderPath}. Please build the LawPortal.Mdb project.");

            _logger.LogInformation("MdbComparison: Reading {Current} vs {Old}", currentMdbPath, oldMdbPath);

            var psi = new ProcessStartInfo
            {
                FileName = _mdbReaderPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("read");
            psi.ArgumentList.Add(currentMdbPath);
            psi.ArgumentList.Add(oldMdbPath);

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

                result.CurrentRowCounts[tableName] = currentRows.Count;
                result.OldRowCounts[tableName] = oldRows.Count;
                result.OldFileRows[tableName] = oldRows.Select(ConvertRow).ToList();

                var keyColumns = TableKeys.ContainsKey(tableName) ? TableKeys[tableName] : new[] { "Id" };

                var diff = CompareTable(tableName, currentRows, oldRows, keyColumns);
                if (diff.AddedRows.Any() || diff.DeletedRows.Any() || diff.ModifiedRows.Any())
                    result.TableDiffs[tableName] = diff;
            }

            // Diagnostic dump — lets us tell "no changes this quarter" apart from
            // "source MDB is missing a table" when a section (e.g. Manual Updates)
            // comes out unexpectedly empty. Logs raw row counts + per-table diff
            // sizes for every table read from either file.
            foreach (var tableName in allTableNames.OrderBy(t => t, StringComparer.OrdinalIgnoreCase))
            {
                int cur = result.CurrentRowCounts.TryGetValue(tableName, out var c) ? c : 0;
                int old = result.OldRowCounts.TryGetValue(tableName, out var o) ? o : 0;
                if (result.TableDiffs.TryGetValue(tableName, out var d))
                    _logger.LogInformation(
                        "MdbComparison {Table}: rows current={Cur} old={Old} | added={Add} modified={Mod} deleted={Del}",
                        tableName, cur, old, d.AddedRows.Count, d.ModifiedRows.Count, d.DeletedRows.Count);
                else
                    _logger.LogInformation(
                        "MdbComparison {Table}: rows current={Cur} old={Old} | no changes", tableName, cur, old);
            }

            return result;
        }

        // MDB path: convert the JSON rows to plain object? dictionaries and diff
        // them through the shared core, so file-sourced and DB-sourced rows go
        // through identical add/delete/modify logic.
        private TableDiff CompareTable(string tableName, List<Dictionary<string, JsonElement>> currentRows,
            List<Dictionary<string, JsonElement>> oldRows, string[] keyColumns)
        {
            return DiffCore(tableName,
                currentRows.Select(ConvertRow).ToList(),
                oldRows.Select(ConvertRow).ToList(),
                keyColumns);
        }

        /// <summary>
        /// Diff two sets of plain rows for a known table by its configured key.
        /// Used to source a table from the database (current live table vs a
        /// quarter snapshot) instead of the MDB files.
        /// </summary>
        public TableDiff CompareObjectRows(string tableName,
            List<Dictionary<string, object?>> currentRows, List<Dictionary<string, object?>> oldRows)
        {
            var keyColumns = TableKeys.TryGetValue(tableName, out var k) ? k : new[] { "Id" };
            return DiffCore(tableName, currentRows, oldRows, keyColumns);
        }

        // Shared add/delete/modify diff over plain rows. Keys and per-column
        // comparison both run through NormalizeObject so a value that differs
        // only in formatting (a date with vs without a midnight time component,
        // 5 vs 5.0, stray whitespace) never splits one logical row into a
        // phantom deleted+added pair.
        private TableDiff DiffCore(string tableName, List<Dictionary<string, object?>> currentRows,
            List<Dictionary<string, object?>> oldRows, string[] keyColumns)
        {
            var diff = new TableDiff { TableName = tableName };

            string Norm(Dictionary<string, object?> row, string col) =>
                row.TryGetValue(col, out var v) ? NormalizeObject(v) : "";
            string GetKey(Dictionary<string, object?> row) =>
                string.Join("|", keyColumns.Select(k => Norm(row, k)));

            var currentByKey = new Dictionary<string, Dictionary<string, object?>>();
            foreach (var row in currentRows) currentByKey[GetKey(row)] = row;

            var oldByKey = new Dictionary<string, Dictionary<string, object?>>();
            foreach (var row in oldRows) oldByKey[GetKey(row)] = row;

            // Added rows (in current but not in old)
            foreach (var kvp in currentByKey)
                if (!oldByKey.ContainsKey(kvp.Key))
                    diff.AddedRows.Add(new RowDiff
                    {
                        Values = kvp.Value,
                        ChangedColumns = new HashSet<string>(kvp.Value.Keys.Where(c => !IgnoreColumns.Contains(c)))
                    });

            // Deleted rows (in old but not in current)
            foreach (var kvp in oldByKey)
                if (!currentByKey.ContainsKey(kvp.Key))
                    diff.DeletedRows.Add(new RowDiff { Values = kvp.Value });

            // Modified rows (in both but with different values)
            foreach (var kvp in currentByKey)
            {
                if (!oldByKey.TryGetValue(kvp.Key, out var oldRow)) continue;
                var newRow = kvp.Value;
                var changedCols = new HashSet<string>();
                foreach (var col in newRow.Keys)
                {
                    if (IgnoreColumns.Contains(col)) continue;
                    if (Norm(newRow, col) != Norm(oldRow, col))
                        changedCols.Add(col);
                }
                if (changedCols.Any())
                    diff.ModifiedRows.Add(new RowDiff
                    {
                        Values = newRow,
                        OldValues = oldRow,
                        ChangedColumns = changedCols
                    });
            }

            return diff;
        }

        // Business identity for "show only the add": the columns that say two rows
        // are the SAME real-world thing even if the full diff key differs. A Law
        // Action is the same deadline when Country/CaseType/ActionType/ActionDue/
        // Indicator match — even if its BasedOn, effective dates, or term changed
        // (all of which are either key columns or body values that a phased change
        // legitimately alters). Tables without an entry here fall back to the
        // stricter "identical body" test.
        private static readonly Dictionary<string, string[]> CollapseIdentity = new(StringComparer.OrdinalIgnoreCase)
        {
            ["tblPatCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "Indicator" },
            ["tblTmkCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "Indicator" },
        };

        /// <summary>
        /// "Show only the add": when a row appears as BOTH a deleted and an added
        /// row for the same real-world entity — because a key column (or, for a
        /// phased deadline, its dates/term) changed — drop the delete and keep the
        /// add. A delete with no matching add is a genuine removal and is left
        /// intact. Matching uses the table's CollapseIdentity when defined (e.g. a
        /// Law Action's deadline identity), otherwise every non-key, non-ignored
        /// column must be identical; tables whose only columns ARE the key are
        /// never collapsed (there is no body to prove sameness).
        /// </summary>
        public static void ShowOnlyReKeyedAdds(MdbComparisonResult result)
        {
            foreach (var kv in result.TableDiffs)
            {
                var diff = kv.Value;
                if (diff.DeletedRows.Count == 0 || diff.AddedRows.Count == 0) continue;

                // ActionParameter is exempt. A parameter's deadline term
                // (ActionDue/Yr/Mo/Dy) is part of its key, so editing it yields a
                // delete + an add that is really ONE modified row — not a removal.
                // The only non-key column is Indicator, so the generic
                // "identical body" test below would match unrelated params (any two
                // sharing a blank Indicator) and wrongly drop the delete, leaving a
                // fully-highlighted "new" parameter. Keep both rows so the Manual
                // Updates renderer can pair them (on ActionDue) into a single
                // modified row that highlights only the changed cell.
                if (kv.Key.EndsWith("ActionParameter", StringComparison.OrdinalIgnoreCase)) continue;

                Func<RowDiff, RowDiff, bool> sameEntity;
                if (CollapseIdentity.TryGetValue(kv.Key, out var idCols))
                {
                    string Id(RowDiff r) => string.Join("|", idCols.Select(c =>
                        r.Values.TryGetValue(c, out var v) ? NormalizeObject(v) : ""));
                    sameEntity = (a, b) => Id(a) == Id(b);
                }
                else
                {
                    var keySet = new HashSet<string>(
                        TableKeys.TryGetValue(kv.Key, out var k) ? k : new[] { "Id" },
                        StringComparer.OrdinalIgnoreCase);
                    sameEntity = (a, b) =>
                    {
                        var cols = a.Values.Keys.Union(b.Values.Keys)
                            .Where(c => !keySet.Contains(c) && !IgnoreColumns.Contains(c))
                            .ToList();
                        if (cols.Count == 0) return false; // key-only table: can't prove sameness
                        foreach (var c in cols)
                        {
                            var av = a.Values.TryGetValue(c, out var x) ? NormalizeObject(x) : "";
                            var bv = b.Values.TryGetValue(c, out var y) ? NormalizeObject(y) : "";
                            if (av != bv) return false;
                        }
                        return true;
                    };
                }

                diff.DeletedRows.RemoveAll(del => diff.AddedRows.Any(add => sameEntity(del, add)));
            }

            // Cross-table: deletions of Expiration / Tax terms are tracked in a
            // separate companion table (…ExpDelete) rather than as DeletedRows of
            // the main …Exp table, so the same-table pass above can't see them. An
            // expiration rule that appears as BOTH a new companion-delete row AND a
            // new main-table row is a phased re-date of one rule (same Type/BasedOn,
            // new effective dates) — show only the add.
            ReconcileDeleteTracker(result, "tblPatCountryExp", "tblPatCountryExpDelete",
                new[] { "Country", "CaseType", "Type", "BasedOn" });
        }

        // Drop rows from a companion delete-tracker table when the same real-world
        // entity also appears as an add/modify in its primary table — i.e. it was
        // re-keyed, not removed. Identity is matched on idCols (which deliberately
        // exclude the columns a phased change alters, e.g. effective dates/term).
        // If the tracker ends up empty its section is dropped entirely.
        private static void ReconcileDeleteTracker(MdbComparisonResult result,
            string primary, string tracker, string[] idCols)
        {
            if (!result.TableDiffs.TryGetValue(tracker, out var td)) return;
            if (td.AddedRows.Count == 0) return;
            if (!result.TableDiffs.TryGetValue(primary, out var pd)) return;

            string Id(RowDiff r) => string.Join("|", idCols.Select(c =>
                r.Values.TryGetValue(c, out var v) ? NormalizeObject(v) : ""));

            var live = new HashSet<string>(pd.AddedRows.Concat(pd.ModifiedRows).Select(Id));
            if (live.Count == 0) return;

            td.AddedRows.RemoveAll(del => live.Contains(Id(del)));
            if (td.AddedRows.Count == 0 && td.ModifiedRows.Count == 0 && td.DeletedRows.Count == 0)
                result.TableDiffs.Remove(tracker);
        }

        /// <summary>
        /// Normalize an already-materialized value (from a DB reader or a
        /// converted JSON row) for comparison — handles type differences,
        /// whitespace, date formatting, number precision, boolean casing.
        /// This is the single source of truth for how two values are judged equal.
        /// </summary>
        private static string NormalizeObject(object? v)
        {
            switch (v)
            {
                case null:
                    return "";
                case bool b:
                    return b ? "true" : "false";
                case sbyte or byte or short or ushort or int or uint or long or ulong:
                    return Convert.ToInt64(v).ToString();
                case float or double or decimal:
                    var d = Convert.ToDouble(v);
                    return d == Math.Floor(d) && !double.IsInfinity(d)
                        ? ((long)d).ToString()
                        : d.ToString("G");
                case DateTime dt:
                    return dt.TimeOfDay == TimeSpan.Zero
                        ? dt.ToString("yyyy-MM-dd")
                        : dt.ToString("yyyy-MM-dd HH:mm:ss");
                case string s:
                    // Dates: strip a midnight time component so "2020-01-01" and
                    // "2020-01-01T00:00:00" compare equal.
                    if (DateTime.TryParse(s, out var pdt))
                        return pdt.TimeOfDay == TimeSpan.Zero
                            ? pdt.ToString("yyyy-MM-dd")
                            : pdt.ToString("yyyy-MM-dd HH:mm:ss");
                    // Whitespace: trim, collapse runs, normalize line endings.
                    s = s.Trim().Replace("\r\n", "\n").Replace("\r", "\n");
                    while (s.Contains("  ")) s = s.Replace("  ", " ");
                    return s;
                default:
                    return v.ToString()?.Trim() ?? "";
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
