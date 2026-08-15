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
        "compobj", "audionation"
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
                if (record is not null) target.Rows.Add(record.Values.ToArray());
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
