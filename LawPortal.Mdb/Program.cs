// LawPortal.Mdb — combined console tool for the Country Law Update workflow.
// Subcommands:
//   read     <mdbPath1> [mdbPath2]    Reader: emit JSON of all tables to stdout.
//   generate <configJsonPath>         Generator: produce per-system MDBs from SQL.
//   import                            Importer: bulk-load MDBs into SQL with system tags.
// All three were previously separate exes (LawPortal.MdbReader / .MdbGenerator / .MdbImporter);
// merged here under a single 32-bit assembly because Reader + Generator both depend on
// Jet/ACE OLEDB which is 32-bit only.

using System;
using System.Threading.Tasks;

namespace LawPortal.Mdb;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 1) return Usage();
        var verb = args[0].ToLowerInvariant();
        var rest = args.Length > 1 ? args[1..] : Array.Empty<string>();
        return verb switch
        {
            "read" => Reader.Run(rest),
            "generate" => await Generator.Run(rest),
            "import" => await Importer.Run(rest),
            _ => Usage(),
        };
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage: LawPortal.Mdb.exe <read|generate|import> [args...]");
        Console.Error.WriteLine("  read     <mdbPath1> [mdbPath2]    Emit MDB contents as JSON.");
        Console.Error.WriteLine("  generate <configJsonPath>         Generate MDBs from SQL Server.");
        Console.Error.WriteLine("  import                            Import MDBs into SQL Server.");
        return 1;
    }
}
