using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text.Json;

namespace LawPortal.Mdb;

// 32-bit MDB reader. Emits JSON of all user tables to stdout:
//   { "file1": { "<tableName>": [ {row}, ... ], ... }, "file2": { ... } }
// Up to two files at once so the comparison service can diff in a single subprocess.
internal static class Reader
{
    public static int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: LawPortal.Mdb.exe read <mdbPath1> [mdbPath2]");
            return 1;
        }

        try
        {
            var result = new Dictionary<string, Dictionary<string, List<Dictionary<string, object?>>>>();

            for (int fileIdx = 0; fileIdx < args.Length && fileIdx < 2; fileIdx++)
            {
                var mdbPath = args[fileIdx];
                if (!File.Exists(mdbPath))
                {
                    Console.Error.WriteLine($"File not found: {mdbPath}");
                    return 1;
                }

                var fileKey = $"file{fileIdx + 1}";
                var tables = new Dictionary<string, List<Dictionary<string, object?>>>();

                var systemMdw = @"C:\Users\hbaeq\AppData\Roaming\Microsoft\Access\System.mdw";
                var connStr = File.Exists(systemMdw)
                    ? $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={mdbPath};Jet OLEDB:System database={systemMdw};User Id=Admin;Password=;"
                    : $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={mdbPath};";
                using var conn = new OleDbConnection(connStr);
                conn.Open();

                // Get table names
                var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null!, null!, null!, "TABLE" });
                var tableNames = new List<string>();
                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        var name = row["TABLE_NAME"]?.ToString() ?? "";
                        if (!name.StartsWith("MSys")) // skip system tables
                            tableNames.Add(name);
                    }
                }

                foreach (var tableName in tableNames)
                {
                    var rows = new List<Dictionary<string, object?>>();
                    try
                    {
                        using var cmd = new OleDbCommand($"SELECT * FROM [{tableName}]", conn);
                        using var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var rowDict = new Dictionary<string, object?>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var colName = reader.GetName(i);
                                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                // Convert dates and other types to strings for JSON
                                if (value is DateTime dt)
                                    value = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                else if (value is byte[] bytes)
                                    value = Convert.ToBase64String(bytes);
                                else if (value is bool b)
                                    value = b;
                                else if (value != null && !(value is string) && !(value is int) && !(value is long) && !(value is double) && !(value is float) && !(value is decimal) && !(value is short) && !(value is bool))
                                    value = value.ToString();
                                rowDict[colName] = value;
                            }
                            rows.Add(rowDict);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Warning: Error reading table {tableName}: {ex.Message}");
                    }
                    tables[tableName] = rows;
                    Console.Error.WriteLine($"Read {tableName}: {rows.Count} rows");
                }

                result[fileKey] = tables;
            }

            // Output JSON to stdout
            var jsonOptions = new JsonSerializerOptions { WriteIndented = false };
            Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
            Console.Out.Flush();

            // Force GC for OleDb cleanup
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
    }
}
