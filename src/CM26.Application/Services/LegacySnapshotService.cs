using System.Text.Json;

namespace CM26.Application.Services;

/// <summary>
/// Writes the FC26 database into a process-neutral snapshot consumed by the
/// original x86 Creation Master forms. The native x64 engine remains the only
/// parser for FC26; no EA data is bundled with the application.
/// </summary>
public static class LegacySnapshotService
{
    private static readonly HashSet<string> IncludedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "nations", "leagues", "leagueteamlinks", "teams", "teamplayerlinks",
        "teamnationlinks", "teamstadiumlinks", "players", "playernames",
        "editedplayernames", "stadiums", "teamkits", "kits", "formations",
        "manager", "referee", "leaguerefereelinks", "teamballs", "shoecolors",
        "playerboots", "fieldpositionboundingboxes", "competition", "competitioninfo",
        "compobj", "audionation", "career_managerpref"
    };

    public static void Write(DatabaseSession session, string outputPath, string? gameRoot = null)
    {
        if (!session.IsLoaded) throw new InvalidOperationException("FC26 database is not loaded.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);

        var snapshot = new LegacySnapshot
        {
            Version = 1,
            GameRoot = gameRoot ?? string.Empty,
            DatabaseFolder = session.LoadedFolder ?? string.Empty,
        };

        // The x64 FC26 engine already includes the verified native Huffman
        // decoder. Resolve names before crossing into the x86 CM16 UI so the
        // legacy process never receives EA's encoded placeholder bytes.
        var playerNames = new DatabasePlayerNameSource(session);

        foreach (var table in session.Tables.Where(t => !t.IsLocale && IncludedTables.Contains(t.Name)))
        {
            var target = new LegacyTable
            {
                Name = table.Name,
                Columns = table.Columns.Select(c => c.Name).ToArray(),
                Rows = new List<string[]>(table.RowCount)
            };
            for (var row = 0; row < table.RowCount; row++)
            {
                var record = session.GetRecord(table.Name, row);
                if (record is null) continue;
                var values = record.Values.ToArray();
                if (table.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase))
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
                target.Rows.Add(values);
            }
            snapshot.Tables.Add(target);
        }

        using var stream = File.Create(outputPath);
        JsonSerializer.Serialize(stream, snapshot, new JsonSerializerOptions { WriteIndented = false });
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
    public string[] Columns { get; set; } = Array.Empty<string>();
    public List<string[]> Rows { get; set; } = new();
}
