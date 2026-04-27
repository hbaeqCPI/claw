using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

// R10.MdbImporter — One-off tool to import MDB data into the database with system assignments.
// Reads MDB files via R10.MdbReader (32-bit), then inserts/merges into SQL Server.

var sqlConnStr = "Server=(local);Database=LawPortalCPiR10;Trusted_Connection=True;Encrypt=false;MultipleActiveResultSets=true;";
var mdbReaderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "R10.MdbReader", "bin", "Debug", "net8.0", "R10.MdbReader.exe");

if (!File.Exists(mdbReaderPath))
{
    Console.Error.WriteLine($"MdbReader not found at: {mdbReaderPath}");
    return 1;
}

// Composite keys per table
var tableKeys = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
{
    ["tblPatCountryLaw"] = new[] { "Country", "CaseType" },
    // CountryDue: add EffStartDate — some rows share the (Country, CaseType, ActionType,
    // ActionDue, BasedOn, Yr, Mo, Dy, Indicator) combo but legitimately differ on
    // effective window (historical vs current).
    ["tblPatCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "BasedOn", "Yr", "Mo", "Dy", "Indicator", "EffStartDate" },
    // CountryExp / CountryExpDelete: add EffBasedOn, EffStartDate, EffEndDate —
    // rows can legitimately share the rest of the key but represent different
    // historical law terms.
    ["tblPatCountryExp"] = new[] { "Country", "CaseType", "Type", "BasedOn", "Yr", "Mo", "Dy", "EffBasedOn", "EffStartDate", "EffEndDate" },
    ["tblPatCountryExpDelete"] = new[] { "Country", "CaseType", "Type", "BasedOn", "Yr", "Mo", "Dy", "EffBasedOn", "EffStartDate", "EffEndDate" },
    ["tblPatDesCaseType"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    ["tblPatArea"] = new[] { "Area" },
    ["tblPatAreaCountry"] = new[] { "Area", "Country" },
    ["tblPatAreaCountryDelete"] = new[] { "Area", "Country" },
    ["tblPatAreaDelete"] = new[] { "Area" },
    ["tblPatCaseType"] = new[] { "CaseType" },
    ["tblPatCountry"] = new[] { "Country" },
    ["tblPatCountryLaw_Ext"] = new[] { "Country", "CaseType" },
    ["tblPatCountryLawUpdate"] = new[] { "Year", "Quarter" },
    ["tblPatDesCaseType_Ext"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    // DesCaseTypeDelete: add IntlCode — same (CaseType, DesCountry, DesCaseType) can
    // appear with different IntlCode (e.g. 'II' vs 'IB').
    ["tblPatDesCaseTypeDelete"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    ["tblPatDesCaseTypeDelete_Ext"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    ["tblPatDesCaseTypeFields"] = new[] { "DesCaseType", "FromField" },
    ["tblPatDesCaseTypeFields_Ext"] = new[] { "DesCaseType", "FromField" },
    ["tblPatDesCaseTypeFieldsDelete"] = new[] { "DesCaseType", "FromField" },
    ["tblPatDesCaseTypeFieldsDelete_Ext"] = new[] { "DesCaseType", "FromField" },
    ["tblTmkCountryLaw"] = new[] { "Country", "CaseType" },
    ["tblTmkCountryDue"] = new[] { "Country", "CaseType", "ActionType", "ActionDue", "BasedOn", "Yr", "Mo", "Dy", "Indicator", "EffStartDate" },
    ["tblTmkDesCaseType"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    ["tblTmkArea"] = new[] { "Area" },
    ["tblTmkAreaCountry"] = new[] { "Area", "Country" },
    ["tblTmkAreaCountryDelete"] = new[] { "Area", "Country" },
    ["tblTmkAreaDelete"] = new[] { "Area" },
    ["tblTmkCaseType"] = new[] { "CaseType" },
    ["tblTmkCountry"] = new[] { "Country" },
    ["tblTmkCountryLawUpdate"] = new[] { "Year", "Quarter" },
    ["tblTmkDesCaseType_Ext"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    ["tblTmkDesCaseTypeDelete"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    ["tblTmkDesCaseTypeDelete_Ext"] = new[] { "IntlCode", "CaseType", "DesCountry", "DesCaseType" },
    ["tblTmkDesCaseTypeFields"] = new[] { "DesCaseType", "FromField" },
    ["tblTmkDesCaseTypeFields_Ext"] = new[] { "DesCaseType", "FromField" },
    ["tblTmkDesCaseTypeFieldsDelete"] = new[] { "DesCaseType", "FromField" },
    ["tblTmkDesCaseTypeFieldsDelete_Ext"] = new[] { "DesCaseType", "FromField" },
};

// Columns to skip during import
var skipColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "tStamp", "Systems", "CountryLawID" };

// MDB files and their system assignments — 2026 Q1 sources on the DevFS share.
// Filenames in the 2000&Up root and R5-7 subfolder have no year prefix for 2026 Q1;
// only the R8&Up variants include "2026_1_".
//   R4            ← 2000&Up\patlaw9.mdb  +  TmkLaw9.mdb
//   PatR5-7       ← R5-7\patlaw9.mdb
//   TmkR5-8       ← R5-7\TmkLaw9.mdb
//   PatR8-R10v2.1 ← R8&Up\2026_1_patlaw10.mdb
//   TmkR9-10v2.2  ← R8&Up\2026_1_TmkLaw10.mdb
// (PatR10v2.2 is handled separately below via SQL_R10v22.)
var devFsBase = @"\\DevFS\CtryLaw\2026\1st_Quarter\2000&Up";
var imports = new[]
{
    new { Path = Path.Combine(devFsBase, "patlaw9.mdb"), System = "R4" },
    new { Path = Path.Combine(devFsBase, "TmkLaw9.mdb"), System = "R4" },
    new { Path = Path.Combine(devFsBase, "R5-7", "patlaw9.mdb"), System = "PatR5-7" },
    new { Path = Path.Combine(devFsBase, "R5-7", "TmkLaw9.mdb"), System = "TmkR5-8" },
    new { Path = Path.Combine(devFsBase, "R8&Up", "2026_1_patlaw10.mdb"), System = "PatR8-R10v2.1" },
    new { Path = Path.Combine(devFsBase, "R8&Up", "2026_1_TmkLaw10.mdb"), System = "TmkR9-10v2.2" },
};

try
{
    using var sqlConn = new SqlConnection(sqlConnStr);
    await sqlConn.OpenAsync();

    // Step 1: Wipe all tblPat* and tblTmk* tables
    Console.WriteLine("=== Step 1: Wiping existing data ===");
    var tablesToWipe = await GetTablesWithSystemsColumn(sqlConn);
    foreach (var t in tablesToWipe)
    {
        await new SqlCommand($"DELETE FROM [{t}]", sqlConn).ExecuteNonQueryAsync();
        Console.WriteLine($"  Cleared {t}");
    }

    // Step 2: Import each MDB
    Console.WriteLine("\n=== Step 2: Importing MDB files ===");
    foreach (var imp in imports)
    {
        Console.WriteLine($"\n--- Importing {Path.GetFileName(imp.Path)} as {imp.System} ---");
        var mdbData = await ReadMdb(mdbReaderPath, imp.Path);
        if (mdbData == null) { Console.Error.WriteLine("  Failed to read MDB"); continue; }

        foreach (var (tableName, rows) in mdbData)
        {
            if (!tableKeys.ContainsKey(tableName)) { Console.WriteLine($"  Skipping {tableName} (no key defined)"); continue; }
            try {
            var keys = tableKeys[tableName];
            int inserted = 0, updated = 0;

            // Get SQL Server column names for this table
            var sqlColsCmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t", sqlConn);
            sqlColsCmd.Parameters.AddWithValue("@t", tableName);
            var sqlColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var rdr = await sqlColsCmd.ExecuteReaderAsync())
            {
                while (await rdr.ReadAsync()) sqlColumns.Add(rdr.GetString(0));
            }

            foreach (var row in rows)
            {
                // Only include columns that exist in both MDB and SQL Server, excluding skip columns
                var columns = row.Keys.Where(c => !skipColumns.Contains(c) && sqlColumns.Contains(c)).ToList();

                // Build key WHERE clause. Yr/Mo/Dy get NULL→0 coercion to match
                // how INSERT writes them (see insertCols loop below) — otherwise
                // "WHERE Yr=NULL" never matches the "Yr=0" row we created earlier
                // and we insert duplicates on each MDB pass.
                object KeyVal(string k)
                {
                    var v = row.ContainsKey(k) ? GetSqlValue(row[k]) : (object?)DBNull.Value;
                    if ((v == null || v == DBNull.Value) &&
                        (k.Equals("Yr", StringComparison.OrdinalIgnoreCase) ||
                         k.Equals("Mo", StringComparison.OrdinalIgnoreCase) ||
                         k.Equals("Dy", StringComparison.OrdinalIgnoreCase)))
                        return 0;
                    return v ?? DBNull.Value;
                }
                // NULL-safe equality — `col = NULL` in SQL is always UNKNOWN, which
                // would cause rows with NULL key columns (e.g. EffStartDate) to never
                // match and get duplicate-inserted on every MDB pass. Wrap each
                // comparison so NULLs on both sides do match.
                var keyWhere = string.Join(" AND ", keys.Select((k, i) =>
                    $"([{k}]=@k{i} OR ([{k}] IS NULL AND @k{i} IS NULL))"));
                var keyParams = keys.Select((k, i) => new SqlParameter($"@k{i}", KeyVal(k))).ToArray();

                // Check if record exists
                var checkCmd = new SqlCommand($"SELECT Systems FROM [{tableName}] WHERE {keyWhere}", sqlConn);
                checkCmd.Parameters.AddRange(keyParams);
                var existingSystems = await checkCmd.ExecuteScalarAsync();

                if (existingSystems != null && existingSystems != DBNull.Value)
                {
                    // Record exists — append system
                    var sys = existingSystems.ToString() ?? "";
                    var sysList = sys.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    if (!sysList.Contains(imp.System))
                    {
                        sysList.Add(imp.System);
                        var newSys = string.Join(",", sysList);
                        var updateCmd = new SqlCommand($"UPDATE [{tableName}] SET Systems=@sys WHERE {keyWhere}", sqlConn);
                        updateCmd.Parameters.AddWithValue("@sys", newSys);
                        updateCmd.Parameters.AddRange(keys.Select((k, i) => new SqlParameter($"@k{i}", KeyVal(k))).ToArray());
                        await updateCmd.ExecuteNonQueryAsync();
                        updated++;
                    }
                }
                else
                {
                    // Insert new record with Systems = this system
                    var insertCols = columns.ToList();
                    if (!insertCols.Contains("Systems")) insertCols.Add("Systems");
                    // Force-include Yr/Mo/Dy if the SQL table has them — some MDB rows
                    // omit these columns entirely, but SQL rejects NOT NULL ints with no
                    // default. The loop below will coerce absent values to 0.
                    foreach (var required in new[] { "Yr", "Mo", "Dy" })
                        if (sqlColumns.Contains(required) &&
                            !insertCols.Contains(required, StringComparer.OrdinalIgnoreCase))
                            insertCols.Add(required);

                    var colList = string.Join(", ", insertCols.Select(c => $"[{c}]"));
                    var paramList = string.Join(", ", insertCols.Select((c, i) => $"@p{i}"));

                    var insertCmd = new SqlCommand($"INSERT INTO [{tableName}] ({colList}) VALUES ({paramList})", sqlConn);
                    for (int i = 0; i < insertCols.Count; i++)
                    {
                        var col = insertCols[i];
                        object? val;
                        if (col == "Systems")
                            val = imp.System;
                        else
                            val = row.ContainsKey(col) ? GetSqlValue(row[col]) : DBNull.Value;
                        // Some MDBs have NULL Yr/Mo/Dy; SQL treats them as NOT NULL int.
                        // Coerce NULL → 0 for these specific columns so the insert succeeds.
                        if ((val == null || val == DBNull.Value) &&
                            (col.Equals("Yr", StringComparison.OrdinalIgnoreCase) ||
                             col.Equals("Mo", StringComparison.OrdinalIgnoreCase) ||
                             col.Equals("Dy", StringComparison.OrdinalIgnoreCase)))
                        {
                            val = 0;
                        }
                        insertCmd.Parameters.AddWithValue($"@p{i}", val ?? DBNull.Value);
                    }

                    try
                    {
                        await insertCmd.ExecuteNonQueryAsync();
                        inserted++;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"  Error inserting into {tableName}: {ex.Message}");
                    }
                }
            }
            Console.WriteLine($"  {tableName}: {inserted} inserted, {updated} updated");
            } catch (Exception ex) { Console.Error.WriteLine($"  ERROR on {tableName}: {ex.Message}"); }
        }
    }

    // Step 3: Import PatR10v2.2 from SQL_R10v22
    Console.WriteLine("\n=== Step 3: Importing PatR10v2.2 from SQL_R10v22 ===");
    await ImportPatR10v22(sqlConn);

    Console.WriteLine("\n=== Done! ===");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

// ---- Helper functions ----

async Task<List<string>> GetTablesWithSystemsColumn(SqlConnection conn)
{
    var tables = new List<string>();
    var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME='Systems' AND (TABLE_NAME LIKE 'tblPat%' OR TABLE_NAME LIKE 'tblTmk%')", conn);
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
    return tables;
}

async Task<Dictionary<string, List<Dictionary<string, JsonElement>>>?> ReadMdb(string readerPath, string mdbPath)
{
    var psi = new ProcessStartInfo
    {
        FileName = readerPath,
        Arguments = $"\"{mdbPath}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(psi);
    if (process == null) return null;

    var stdout = await process.StandardOutput.ReadToEndAsync();
    var stderr = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    if (!string.IsNullOrWhiteSpace(stderr))
        Console.Error.WriteLine(stderr);

    if (string.IsNullOrWhiteSpace(stdout)) return null;

    var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Dictionary<string, JsonElement>>>>>(stdout);
    return data?.ContainsKey("file1") == true ? data["file1"] : null;
}

object? GetSqlValue(JsonElement el)
{
    return el.ValueKind switch
    {
        JsonValueKind.Null => DBNull.Value,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Number => el.TryGetInt64(out var l) ? (object)l : el.GetDouble(),
        JsonValueKind.String => ParseStringValue(el.GetString()),
        _ => el.ToString()
    };
}

object? ParseStringValue(string? s)
{
    if (s == null) return DBNull.Value;
    if (s.Contains(":") && DateTime.TryParse(s, out var dt)) return dt;
    return s;
}

async Task ImportPatR10v22(SqlConnection destConn)
{
    using var srcConn = new SqlConnection("Server=(local);Database=SQL_R10v22;Trusted_Connection=True;Encrypt=false;MultipleActiveResultSets=true;");
    await srcConn.OpenAsync();

    // Import tblPatCountryLaw (including LabelTaxSched)
    var clCmd = new SqlCommand(@"SELECT Country, CaseType, DefaultAgent, AutoGenDesCtry, AutoUpdtDesPatRecs, CalcExpirBeforeIssue,
        Remarks, UserRemarks, LabelTaxSched, CreatedBy, UpdatedBy, DateCreated, LastUpdate
        FROM tblPatCountryLaw", srcConn);
    using var clReader = await clCmd.ExecuteReaderAsync();
    int clCount = 0;
    while (await clReader.ReadAsync())
    {
        var country = clReader["Country"]?.ToString() ?? "";
        var caseType = clReader["CaseType"]?.ToString() ?? "";

        // Check if exists
        var checkCmd = new SqlCommand("SELECT Systems FROM tblPatCountryLaw WHERE Country=@c AND CaseType=@ct", destConn);
        checkCmd.Parameters.AddWithValue("@c", country);
        checkCmd.Parameters.AddWithValue("@ct", caseType);
        var existing = await checkCmd.ExecuteScalarAsync();

        if (existing != null && existing != DBNull.Value)
        {
            // Append system
            var sys = existing.ToString() ?? "";
            if (!sys.Split(',').Select(s => s.Trim()).Contains("PatR10v2.2", StringComparer.OrdinalIgnoreCase))
            {
                var newSys = string.IsNullOrEmpty(sys) ? "PatR10v2.2" : sys + ",PatR10v2.2";
                var updCmd = new SqlCommand(@"UPDATE tblPatCountryLaw SET Systems=@sys, LabelTaxSched=@lts
                    WHERE Country=@c AND CaseType=@ct", destConn);
                updCmd.Parameters.AddWithValue("@sys", newSys);
                updCmd.Parameters.AddWithValue("@lts", clReader["LabelTaxSched"] ?? DBNull.Value);
                updCmd.Parameters.AddWithValue("@c", country);
                updCmd.Parameters.AddWithValue("@ct", caseType);
                await updCmd.ExecuteNonQueryAsync();
            }
        }
        else
        {
            var insCmd = new SqlCommand(@"INSERT INTO tblPatCountryLaw
                (Country, CaseType, DefaultAgent, AutoGenDesCtry, AutoUpdtDesPatRecs, CalcExpirBeforeIssue,
                 Remarks, UserRemarks, LabelTaxSched, Systems, UserID, DateCreated, LastUpdate)
                VALUES (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12)", destConn);
            insCmd.Parameters.AddWithValue("@p0", country);
            insCmd.Parameters.AddWithValue("@p1", caseType);
            insCmd.Parameters.AddWithValue("@p2", clReader["DefaultAgent"] is DBNull ? "" : clReader["DefaultAgent"]?.ToString() ?? "");
            insCmd.Parameters.AddWithValue("@p3", clReader["AutoGenDesCtry"]);
            insCmd.Parameters.AddWithValue("@p4", clReader["AutoUpdtDesPatRecs"]);
            insCmd.Parameters.AddWithValue("@p5", clReader["CalcExpirBeforeIssue"]);
            insCmd.Parameters.AddWithValue("@p6", clReader["Remarks"] is DBNull ? "" : clReader["Remarks"]?.ToString() ?? "");
            insCmd.Parameters.AddWithValue("@p7", clReader["UserRemarks"] is DBNull ? "" : clReader["UserRemarks"]?.ToString() ?? "");
            insCmd.Parameters.AddWithValue("@p8", clReader["LabelTaxSched"] ?? DBNull.Value);
            insCmd.Parameters.AddWithValue("@p9", "PatR10v2.2");
            insCmd.Parameters.AddWithValue("@p10", clReader["CreatedBy"] is DBNull ? "" : clReader["CreatedBy"]?.ToString() ?? "");
            insCmd.Parameters.AddWithValue("@p11", clReader["DateCreated"] is DBNull ? DBNull.Value : clReader["DateCreated"]);
            insCmd.Parameters.AddWithValue("@p12", clReader["LastUpdate"] is DBNull ? DBNull.Value : clReader["LastUpdate"]);
            await insCmd.ExecuteNonQueryAsync();
        }
        clCount++;
    }
    Console.WriteLine($"  tblPatCountryLaw: {clCount} rows processed from SQL_R10v22");

    // Import tblPatCountryDue (including MultipleBasedOn)
    var dueCmd = new SqlCommand(@"SELECT Country, CaseType, ActionType, ActionDue, BasedOn, Yr, Mo, Dy, Indicator, Recurring,
        EffBasedOn, EffStartDate, EffEndDate, CPIAction, [Calculate], MultipleBasedOn, CPIPermanentID,
        CreatedBy, UpdatedBy, DateCreated, LastUpdate
        FROM tblPatCountryDue", srcConn);
    using var dueReader = await dueCmd.ExecuteReaderAsync();
    int dueCount = 0;
    while (await dueReader.ReadAsync())
    {
        var country = dueReader["Country"]?.ToString() ?? "";
        var caseType = dueReader["CaseType"]?.ToString() ?? "";
        var actionType = dueReader["ActionType"]?.ToString() ?? "";
        var actionDue = dueReader["ActionDue"]?.ToString() ?? "";
        var basedOn = dueReader["BasedOn"]?.ToString() ?? "";

        // Widen the match with EffStartDate so the UPDATE below doesn't over-match
        // sibling rows that share the 9-col key but represent a different effective
        // window. Without this, the UPDATE overwrote the Systems CSV of unrelated
        // rows (see earlier report's "+4 overcount" bug).
        var effStartDate = dueReader["EffStartDate"];
        var effParamNullSafe = "(EffStartDate=@esd OR (EffStartDate IS NULL AND @esd IS NULL))";
        var checkCmd = new SqlCommand($@"SELECT Systems FROM tblPatCountryDue
            WHERE Country=@c AND CaseType=@ct AND ActionType=@at AND ActionDue=@ad AND BasedOn=@bo
            AND Yr=@yr AND Mo=@mo AND Dy=@dy AND Indicator=@ind AND {effParamNullSafe}", destConn);
        checkCmd.Parameters.AddWithValue("@c", country);
        checkCmd.Parameters.AddWithValue("@ct", caseType);
        checkCmd.Parameters.AddWithValue("@at", actionType);
        checkCmd.Parameters.AddWithValue("@ad", actionDue);
        checkCmd.Parameters.AddWithValue("@bo", basedOn);
        checkCmd.Parameters.AddWithValue("@yr", dueReader["Yr"]);
        checkCmd.Parameters.AddWithValue("@mo", dueReader["Mo"]);
        checkCmd.Parameters.AddWithValue("@dy", dueReader["Dy"]);
        checkCmd.Parameters.AddWithValue("@ind", dueReader["Indicator"] ?? "");
        checkCmd.Parameters.AddWithValue("@esd", effStartDate ?? DBNull.Value);
        var existing = await checkCmd.ExecuteScalarAsync();

        if (existing != null && existing != DBNull.Value)
        {
            var sys = existing.ToString() ?? "";
            if (!sys.Split(',').Select(s => s.Trim()).Contains("PatR10v2.2", StringComparer.OrdinalIgnoreCase))
            {
                var newSys = string.IsNullOrEmpty(sys) ? "PatR10v2.2" : sys + ",PatR10v2.2";
                var updCmd = new SqlCommand($@"UPDATE tblPatCountryDue SET Systems=@sys, MultipleBasedOn=@mbo
                    WHERE Country=@c AND CaseType=@ct AND ActionType=@at AND ActionDue=@ad AND BasedOn=@bo
                    AND Yr=@yr AND Mo=@mo AND Dy=@dy AND Indicator=@ind AND {effParamNullSafe}", destConn);
                updCmd.Parameters.AddWithValue("@sys", newSys);
                updCmd.Parameters.AddWithValue("@mbo", dueReader["MultipleBasedOn"]);
                updCmd.Parameters.AddWithValue("@c", country);
                updCmd.Parameters.AddWithValue("@ct", caseType);
                updCmd.Parameters.AddWithValue("@at", actionType);
                updCmd.Parameters.AddWithValue("@ad", actionDue);
                updCmd.Parameters.AddWithValue("@bo", basedOn);
                updCmd.Parameters.AddWithValue("@yr", dueReader["Yr"]);
                updCmd.Parameters.AddWithValue("@mo", dueReader["Mo"]);
                updCmd.Parameters.AddWithValue("@dy", dueReader["Dy"]);
                updCmd.Parameters.AddWithValue("@ind", dueReader["Indicator"] ?? "");
                updCmd.Parameters.AddWithValue("@esd", effStartDate ?? DBNull.Value);
                await updCmd.ExecuteNonQueryAsync();
            }
        }
        else
        {
            // Get next CDueId
            var maxIdCmd = new SqlCommand("SELECT ISNULL(MAX(CDueId),0)+1 FROM tblPatCountryDue", destConn);
            var nextId = (int)(await maxIdCmd.ExecuteScalarAsync())!;

            var insCmd = new SqlCommand(@"INSERT INTO tblPatCountryDue
                (CDueId, Country, CaseType, ActionType, ActionDue, BasedOn, Yr, Mo, Dy, Indicator, Recurring,
                 EffBasedOn, EffStartDate, EffEndDate, CPIAction, [Calculate], MultipleBasedOn, CPIPermanentID,
                 Systems, UserID, DateCreated, LastUpdate)
                VALUES (@id,@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20)", destConn);
            insCmd.Parameters.AddWithValue("@id", nextId);
            insCmd.Parameters.AddWithValue("@p0", country);
            insCmd.Parameters.AddWithValue("@p1", caseType);
            insCmd.Parameters.AddWithValue("@p2", actionType);
            insCmd.Parameters.AddWithValue("@p3", actionDue);
            insCmd.Parameters.AddWithValue("@p4", basedOn);
            insCmd.Parameters.AddWithValue("@p5", dueReader["Yr"]);
            insCmd.Parameters.AddWithValue("@p6", dueReader["Mo"]);
            insCmd.Parameters.AddWithValue("@p7", dueReader["Dy"]);
            insCmd.Parameters.AddWithValue("@p8", dueReader["Indicator"] ?? "");
            insCmd.Parameters.AddWithValue("@p9", dueReader["Recurring"]);
            insCmd.Parameters.AddWithValue("@p10", dueReader["EffBasedOn"] is DBNull ? "" : dueReader["EffBasedOn"]?.ToString() ?? "");
            insCmd.Parameters.AddWithValue("@p11", dueReader["EffStartDate"] is DBNull ? DBNull.Value : dueReader["EffStartDate"]);
            insCmd.Parameters.AddWithValue("@p12", dueReader["EffEndDate"] is DBNull ? DBNull.Value : dueReader["EffEndDate"]);
            insCmd.Parameters.AddWithValue("@p13", dueReader["CPIAction"]);
            insCmd.Parameters.AddWithValue("@p14", dueReader["Calculate"]);
            insCmd.Parameters.AddWithValue("@p15", dueReader["MultipleBasedOn"]);
            insCmd.Parameters.AddWithValue("@p16", dueReader["CPIPermanentID"] is DBNull ? DBNull.Value : dueReader["CPIPermanentID"]);
            insCmd.Parameters.AddWithValue("@p17", "PatR10v2.2");
            insCmd.Parameters.AddWithValue("@p18", dueReader["CreatedBy"] is DBNull ? "" : dueReader["CreatedBy"]?.ToString() ?? "");
            insCmd.Parameters.AddWithValue("@p19", dueReader["DateCreated"] is DBNull ? DateTime.Now : dueReader["DateCreated"]);
            insCmd.Parameters.AddWithValue("@p20", dueReader["LastUpdate"] is DBNull ? DateTime.Now : dueReader["LastUpdate"]);
            await insCmd.ExecuteNonQueryAsync();
        }
        dueCount++;
    }
    Console.WriteLine($"  tblPatCountryDue: {dueCount} rows processed from SQL_R10v22");
}
