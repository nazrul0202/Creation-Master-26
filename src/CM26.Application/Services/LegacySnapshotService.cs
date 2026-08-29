using System.IO.Compression;
using System.Text.Json;

namespace CM26.Application.Services;

/// <summary>
/// Writes the FC26 database into a process-neutral snapshot consumed by the
/// original x86 Creation Master forms. The native x64 engine remains the only
/// parser for FC26; no EA data is bundled with the application.
/// </summary>
public static class LegacySnapshotService
{
    internal static readonly HashSet<string> CoreTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "nations", "leagues", "leagueteamlinks", "teams", "teamplayerlinks",
        "teamnationlinks", "teamstadiumlinks", "players", "playernames",
        "stadiums", "teamkits", "kits", "formations",
        "default_mentalities", "defaultteamdata",
        "default_teamsheets", "manager", "referee", "leaguerefereelinks",
        "teamballs", "playerboots", "fieldpositionboundingboxes", "competition"
    };

    public static void Write(DatabaseSession session, string outputPath, string? gameRoot = null)
    {
        if (!session.IsLoaded) throw new InvalidOperationException("FC26 database is not loaded.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);

        var fullOutputPath = Path.GetFullPath(outputPath);
        var dataDirectory = fullOutputPath + ".tables";
        var temporaryDataDirectory = dataDirectory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temporaryDataDirectory);

        var snapshot = new LegacySnapshot
        {
            Version = 2,
            GameRoot = gameRoot ?? string.Empty,
            DatabaseFolder = session.LoadedFolder ?? string.Empty,
        };

        // The x64 FC26 engine already includes the verified native Huffman
        // decoder. Resolve names before crossing into the x86 CM16 UI so the
        // legacy process never receives EA's encoded placeholder bytes.
        var playerNames = new DatabasePlayerNameSource(session);

        // The named set above is still the curated set consumed by the strongly
        // typed CM16 editors. The Advanced Database Workspace sees every table
        // exposed by the verified native engine. Locale tables use an explicit
        // display prefix so names shared with the main DB can never collide.
        var tableIndex = 0;
        foreach (var table in session.Tables)
        {
            var dataFileName = tableIndex.ToString("D4") + ".json.gz";
            var target = new LegacyTable
            {
                Name = table.IsLocale ? "locale::" + table.Name : table.Name,
                SourceName = table.Name,
                IsLocale = table.IsLocale,
                IsCore = !table.IsLocale && CoreTables.Contains(table.Name),
                Columns = table.Columns.Select(c => c.Name).ToArray(),
                ColumnDetails = table.Columns.Select(c => new LegacyColumn
                {
                    Name = c.Name,
                    IsWritable = c.IsWritable,
                    Kind = c.Kind,
                    Depth = c.Depth,
                    RangeLow = c.RangeLow,
                    RangeHigh = c.RangeHigh,
                }).ToList(),
                RowCount = table.RowCount,
                DataFile = Path.GetFileName(dataDirectory) + "/" + dataFileName,
            };

            var tableFile = Path.Combine(temporaryDataDirectory, dataFileName);
            var exportedRows = 0;
            using (var file = File.Create(tableFile))
            using (var compressed = new GZipStream(file, CompressionLevel.Optimal, leaveOpen: false))
            using (var json = new Utf8JsonWriter(compressed))
            {
                json.WriteStartArray();
                for (var row = 0; row < table.RowCount; row++)
                {
                    var record = session.GetRecord(table.Name, row);
                    if (record is null) continue;
                    var values = record.Values.ToArray();
                    if (!table.IsLocale && table.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase))
                    {
                        var idColumn = Array.FindIndex(target.Columns,
                            column => column.Equals("nameid", StringComparison.OrdinalIgnoreCase));
                        var nameColumn = Array.FindIndex(target.Columns,
                            column => column.Equals("name", StringComparison.OrdinalIgnoreCase));
                        if (idColumn >= 0 && nameColumn >= 0 &&
                            int.TryParse(values[idColumn], out var nameId))
                        {
                            var decoded = playerNames.NameById(nameId);
                            if (!string.IsNullOrWhiteSpace(decoded)) values[nameColumn] = decoded;
                        }
                    }
                    JsonSerializer.Serialize(json, values);
                    exportedRows++;
                }
                json.WriteEndArray();
            }
            target.RowCount = exportedRows;
            snapshot.Tables.Add(target);
            tableIndex++;
        }

        var temporaryManifest = fullOutputPath + ".tmp-" + Guid.NewGuid().ToString("N");
        using (var stream = File.Create(temporaryManifest))
            JsonSerializer.Serialize(stream, snapshot, new JsonSerializerOptions { WriteIndented = false });

        if (Directory.Exists(dataDirectory)) Directory.Delete(dataDirectory, recursive: true);
        Directory.Move(temporaryDataDirectory, dataDirectory);
        File.Move(temporaryManifest, fullOutputPath, overwrite: true);
    }
}

public sealed class LegacySnapshot
{
    public int Version { get; set; }
    public string GameRoot { get; set; } = string.Empty;
    public string DatabaseFolder { get; set; } = string.Empty;
    public List<LegacyTable> Tables { get; set; } = new();
}

public sealed class LegacyTable
{
    public string Name { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public bool IsLocale { get; set; }
    public bool IsCore { get; set; }
    public string[] Columns { get; set; } = Array.Empty<string>();
    public List<LegacyColumn> ColumnDetails { get; set; } = new();
    public int RowCount { get; set; }
    public string DataFile { get; set; } = string.Empty;
    public List<string[]> Rows { get; set; } = new();
}

public sealed class LegacyColumn
{
    public string Name { get; set; } = string.Empty;
    public bool IsWritable { get; set; }
    public int Kind { get; set; }
    public int Depth { get; set; }
    public long RangeLow { get; set; }
    public long RangeHigh { get; set; }
}
