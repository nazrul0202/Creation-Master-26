using System;
using System.IO;
using System.Linq;
using FifaLibrary;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: FifaLibrary16Compatibility <fifa_ng_db-meta.xml> <fifa_ng_db.db>");
            return 64;
        }

        var metaPath = Path.GetFullPath(args[0]);
        var dbPath = Path.GetFullPath(args[1]);
        if (!File.Exists(metaPath) || !File.Exists(dbPath))
        {
            Console.Error.WriteLine("Input file missing.");
            return 66;
        }

        try
        {
            // Constructor only reads input. This probe deliberately never calls SaveDb or SaveXml.
            var database = new DbFile(dbPath, metaPath);
            Console.WriteLine("RESULT=OPENED");
            Console.WriteLine("TABLES=" + database.NTables);

            foreach (var table in database.Table.Where(table => table != null)
                         .OrderBy(table => table.TableDescriptor.TableName, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"TABLE={table.TableDescriptor.TableName}|ROWS={table.NRecords}|FIELDS={table.TableDescriptor.NFields}");
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("RESULT=FAILED");
            Console.Error.WriteLine(ex.GetType().FullName + ": " + ex.Message);
            return 1;
        }
    }
}
