using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.SqlClient;

// R10.MdbGenerator - 32-bit console app for generating Access database files
// Usage: R10.MdbGenerator.exe <configJsonPath>
// Config JSON: { "SqlConnectionString", "TemplatePath", "OutputFolder", "Systems", "GeneratePatent", "GenerateTrademark" }

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: R10.MdbGenerator.exe <configJsonPath>");
    return 1;
}

var configPath = args[0];
if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"Config file not found: {configPath}");
    return 1;
}

try
{
    var configJson = File.ReadAllText(configPath);
    var config = JsonSerializer.Deserialize<GeneratorConfig>(configJson);
    if (config == null)
    {
        Console.Error.WriteLine("Failed to parse config JSON.");
        return 1;
    }

    var generator = new MdbGenerator(config);
    var files = await generator.GenerateAsync();

    // Output generated file paths (one per line) for the calling process to read
    foreach (var f in files)
        Console.WriteLine(f);

    // Write success marker so caller can detect success even if ACE provider crashes during cleanup
    Console.WriteLine("__MDB_SUCCESS__");
    Console.Out.Flush();

    // Force GC to clean up OleDb COM objects before process exit (avoids ACE provider crash)
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

// ---- Classes ----

public class GeneratorConfig
{
    public string SqlConnectionString { get; set; } = "";
    public string TemplatePath { get; set; } = "";
    public string OutputFolder { get; set; } = "";
    public string Systems { get; set; } = "";
    public bool GeneratePatent { get; set; }
    public bool GenerateTrademark { get; set; }
    public string ReleaseName { get; set; } = "";
}

public class ColumnInfo
{
    public string Name { get; set; } = "";
    public string DataType { get; set; } = "";
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public int? NumericPrecision { get; set; }
    public int? NumericScale { get; set; }
}

public class MdbGenerator
{
    private readonly GeneratorConfig _config;

    // Full patent tables (used by 97, PatR5-7, PatR8-10)
    private static readonly string[] PatentTablesFull = new[]
    {
        "tblPatArea", "tblPatAreaCountry", "tblPatAreaCountryDelete", "tblPatAreaDelete",
        "tblPatCaseType", "tblPatCountry", "tblPatCountryDue", "tblPatCountryExp",
        "tblPatCountryExpDelete", "tblPatCountryLaw", "tblPatCountryLaw_Ext", "tblPatCountryLawUpdate",
        "tblPatDesCaseType", "tblPatDesCaseType_Ext", "tblPatDesCaseTypeDelete", "tblPatDesCaseTypeDelete_Ext",
        "tblPatDesCaseTypeFields", "tblPatDesCaseTypeFields_Ext", "tblPatDesCaseTypeFieldsDelete", "tblPatDesCaseTypeFieldsDelete_Ext"
    };

    // Full trademark tables (used by 97, TmkR5-8, TmkR9-10)
    private static readonly string[] TrademarkTablesFull = new[]
    {
        "tblTmkArea", "tblTmkAreaCountry", "tblTmkAreaCountryDelete", "tblTmkAreaDelete",
        "tblTmkCaseType", "tblTmkCountry", "tblTmkCountryDue", "tblTmkCountryLaw",
        "tblTmkCountryLawUpdate", "tblTmkDesCaseType", "tblTmkDesCaseType_Ext",
        "tblTmkDesCaseTypeDelete", "tblTmkDesCaseTypeDelete_Ext",
        "tblTmkDesCaseTypeFields", "tblTmkDesCaseTypeFields_Ext",
        "tblTmkDesCaseTypeFieldsDelete", "tblTmkDesCaseTypeFieldsDelete_Ext"
    };

    // R4 patent tables (no _Ext tables)
    private static readonly string[] PatentTables2000 = new[]
    {
        "tblPatArea", "tblPatAreaCountry", "tblPatAreaCountryDelete", "tblPatAreaDelete",
        "tblPatCaseType", "tblPatCountry", "tblPatCountryDue", "tblPatCountryExp",
        "tblPatCountryExpDelete", "tblPatCountryLaw", "tblPatCountryLawUpdate",
        "tblPatDesCaseType", "tblPatDesCaseTypeDelete",
        "tblPatDesCaseTypeFields", "tblPatDesCaseTypeFieldsDelete"
    };

    // R4 trademark tables (no _Ext tables)
    private static readonly string[] TrademarkTables2000 = new[]
    {
        "tblTmkArea", "tblTmkAreaCountry", "tblTmkAreaCountryDelete", "tblTmkAreaDelete",
        "tblTmkCaseType", "tblTmkCountry", "tblTmkCountryDue", "tblTmkCountryLaw",
        "tblTmkCountryLawUpdate", "tblTmkDesCaseType", "tblTmkDesCaseTypeDelete",
        "tblTmkDesCaseTypeFields", "tblTmkDesCaseTypeFieldsDelete"
    };

    private static string[] GetPatentTables(string systemType) =>
        systemType.Equals("R4", StringComparison.OrdinalIgnoreCase) ? PatentTables2000 : PatentTablesFull;

    private static string[] GetTrademarkTables(string systemType) =>
        systemType.Equals("R4", StringComparison.OrdinalIgnoreCase) ? TrademarkTables2000 : TrademarkTablesFull;

    // Per-table column whitelist: only export these columns for the specified tables
    private static readonly Dictionary<string, HashSet<string>> TableColumnWhitelist = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tblTmkCaseType"] = new(StringComparer.OrdinalIgnoreCase) { "CaseType", "Description" }
    };

    // Columns that only exist for PatR10v2.2 — excluded from all other systems
    private static readonly Dictionary<string, HashSet<string>> PatR10OnlyColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tblPatCountryLaw"] = new(StringComparer.OrdinalIgnoreCase) { "LabelTaxSched" },
        ["tblPatCountryDue"] = new(StringComparer.OrdinalIgnoreCase) { "MultipleBasedOn" }
    };

    private static bool IsPatR10System(string systemType) =>
        systemType.Equals("PatR10v2.2", StringComparison.OrdinalIgnoreCase);

    public MdbGenerator(GeneratorConfig config)
    {
        _config = config;
    }

    public async Task<List<string>> GenerateAsync()
    {
        var generatedFiles = new List<string>();
        var selectedSystems = (_config.Systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Directory.CreateDirectory(_config.OutputFolder);

        // Build file name prefix from release name (sanitize for filesystem)
        var namePrefix = string.IsNullOrWhiteSpace(_config.ReleaseName)
            ? "Release"
            : string.Join("", _config.ReleaseName.Split(Path.GetInvalidFileNameChars()));

        var systemType = (_config.Systems ?? "").Trim(); // Systems field carries the system type name

        if (_config.GeneratePatent)
        {
            var path = Path.Combine(_config.OutputFolder, $"{namePrefix}-Pat.mdb");
            if (File.Exists(path)) File.Delete(path);
            await GenerateMdb(path, GetPatentTables(systemType), selectedSystems, systemType);
            generatedFiles.Add(path);
        }

        if (_config.GenerateTrademark)
        {
            var path = Path.Combine(_config.OutputFolder, $"{namePrefix}-Tmk.mdb");
            if (File.Exists(path)) File.Delete(path);
            await GenerateMdb(path, GetTrademarkTables(systemType), selectedSystems, systemType);
            generatedFiles.Add(path);
        }

        return generatedFiles;
    }

    private async Task GenerateMdb(string mdbPath, string[] tables, HashSet<string> selectedSystems, string systemType = "")
    {
        if (!File.Exists(_config.TemplatePath))
            throw new FileNotFoundException($"Blank Access template not found at: {_config.TemplatePath}");

        File.Copy(_config.TemplatePath, mdbPath, true);

        var oleConnString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={mdbPath};";

        using var sqlConn = new SqlConnection(_config.SqlConnectionString);
        await sqlConn.OpenAsync();

        using var oleConn = new OleDbConnection(oleConnString);
        oleConn.Open();

        foreach (var tableName in tables)
        {
            try
            {
                // Check if table exists in SQL Server
                using var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @t", sqlConn);
                checkCmd.Parameters.AddWithValue("@t", tableName);
                var exists = (int)await checkCmd.ExecuteScalarAsync()! > 0;
                if (!exists)
                {
                    Console.Error.WriteLine($"Warning: Table {tableName} not found, skipping.");
                    continue;
                }

                // Get column info
                var columns = await GetTableColumns(sqlConn, tableName);
                if (!columns.Any()) continue;

                bool hasSystemsColumn = columns.Any(c =>
                    c.Name.Equals("Systems", StringComparison.OrdinalIgnoreCase));

                // Filter out timestamp/rowversion columns and the Systems column itself
                var exportColumns = columns.Where(c =>
                    !c.DataType.Equals("timestamp", StringComparison.OrdinalIgnoreCase) &&
                    !c.DataType.Equals("rowversion", StringComparison.OrdinalIgnoreCase) &&
                    !c.Name.Equals("Systems", StringComparison.OrdinalIgnoreCase)).ToList();

                // Apply per-table column whitelist if defined
                if (TableColumnWhitelist.TryGetValue(tableName, out var whitelist))
                    exportColumns = exportColumns.Where(c => whitelist.Contains(c.Name)).ToList();

                // Exclude PatR10v2.2-only columns for all other systems
                if (!IsPatR10System(systemType) && PatR10OnlyColumns.TryGetValue(tableName, out var r10Only))
                    exportColumns = exportColumns.Where(c => !r10Only.Contains(c.Name)).ToList();

                // Create table in Access (without Systems column)
                var createSql = BuildCreateTableSql(tableName, exportColumns);
                using (var oleCmd = new OleDbCommand(createSql, oleConn))
                {
                    oleCmd.ExecuteNonQuery();
                }

                // Read and insert data
                using var dataCmd = new SqlCommand($"SELECT * FROM [{tableName}]", sqlConn);
                using var reader = await dataCmd.ExecuteReaderAsync();

                var systemsOrdinal = hasSystemsColumn ? reader.GetOrdinal("Systems") : -1;
                int rowCount = 0;

                while (await reader.ReadAsync())
                {
                    // Strict system filtering: only include records whose Systems field
                    // contains at least one of the selected systems
                    if (hasSystemsColumn && selectedSystems.Any())
                    {
                        var rowSystems = reader.IsDBNull(systemsOrdinal) ? "" : reader.GetString(systemsOrdinal);
                        if (string.IsNullOrWhiteSpace(rowSystems))
                            continue; // Skip records with no system assigned

                        var rowIndividualSystems = rowSystems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (!selectedSystems.Any(s => rowIndividualSystems.Contains(s)))
                            continue; // Skip records that don't match any selected system
                    }

                    // Insert row using the same export columns (no timestamp, no Systems)
                    var insertSql = BuildInsertSql(tableName, exportColumns);
                    using var insertCmd = new OleDbCommand(insertSql, oleConn);
                    for (int i = 0; i < exportColumns.Count; i++)
                    {
                        var ordinal = reader.GetOrdinal(exportColumns[i].Name);
                        object value = reader.IsDBNull(ordinal) ? DBNull.Value : reader.GetValue(ordinal);

                        // Fix DateTime millisecond precision - Access doesn't support sub-second precision
                        // and OleDb AddWithValue infers DBTimeStamp which causes "Data type mismatch"
                        if (value is DateTime dt)
                        {
                            value = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                        }

                        // Fix boolean values for Access BIT columns
                        if (value is bool boolVal)
                        {
                            value = boolVal ? (short)-1 : (short)0;
                        }

                        insertCmd.Parameters.AddWithValue($"@p{i}", value ?? DBNull.Value);
                    }
                    insertCmd.ExecuteNonQuery();
                    rowCount++;
                }

                Console.Error.WriteLine($"Table {tableName}: {rowCount} rows exported.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error exporting table {tableName}: {ex.Message}");
            }
        }
    }

    private async Task<List<ColumnInfo>> GetTableColumns(SqlConnection conn, string tableName)
    {
        var columns = new List<ColumnInfo>();
        var sql = @"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE, NUMERIC_PRECISION, NUMERIC_SCALE
                    FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t ORDER BY ORDINAL_POSITION";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@t", tableName);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new ColumnInfo
            {
                Name = reader.GetString(0),
                DataType = reader.GetString(1),
                MaxLength = reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2),
                IsNullable = reader.GetString(3) == "YES",
                NumericPrecision = reader.IsDBNull(4) ? null : (int?)Convert.ToInt32(reader.GetValue(4)),
                NumericScale = reader.IsDBNull(5) ? null : (int?)Convert.ToInt32(reader.GetValue(5))
            });
        }
        return columns;
    }

    private string BuildCreateTableSql(string tableName, List<ColumnInfo> columns)
    {
        var colDefs = columns.Select(c => $"[{c.Name}] {MapSqlTypeToOleDb(c)}");
        return $"CREATE TABLE [{tableName}] ({string.Join(", ", colDefs)})";
    }

    private string BuildInsertSql(string tableName, List<ColumnInfo> columns)
    {
        var colNames = string.Join(", ", columns.Select(c => $"[{c.Name}]"));
        var paramNames = string.Join(", ", columns.Select((c, i) => $"@p{i}"));
        return $"INSERT INTO [{tableName}] ({colNames}) VALUES ({paramNames})";
    }

    private string MapSqlTypeToOleDb(ColumnInfo col)
    {
        switch (col.DataType.ToLower())
        {
            case "nvarchar":
            case "varchar":
                var len = col.MaxLength ?? 255;
                if (len < 0 || len > 255) return "MEMO";
                return $"TEXT({len})";
            case "nchar":
            case "char":
                return $"TEXT({col.MaxLength ?? 50})";
            case "int":
                return "LONG";
            case "smallint":
                return "SHORT";
            case "tinyint":
                return "BYTE";
            case "bigint":
                return "LONG";
            case "bit":
                return "BIT";
            case "decimal":
            case "numeric":
            case "money":
            case "smallmoney":
                return "CURRENCY";
            case "float":
            case "real":
                return "DOUBLE";
            case "datetime":
            case "datetime2":
            case "date":
            case "smalldatetime":
                return "DATETIME";
            case "uniqueidentifier":
                return "TEXT(36)";
            case "varbinary":
            case "binary":
            case "image":
                return "OLEOBJECT";
            case "text":
            case "ntext":
                return "MEMO";
            case "timestamp":
            case "rowversion":
                return "OLEOBJECT";
            default:
                return "TEXT(255)";
        }
    }
}
