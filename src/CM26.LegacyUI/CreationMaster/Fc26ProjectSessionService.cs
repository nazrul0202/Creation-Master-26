using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CreationMaster;

internal static class Fc26ProjectSessionService
{
    private static readonly string RecentPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "recent-projects.json");

    internal static void Save(string fileName)
    {
        if (!Fc26SnapshotLoader.IsLoaded) throw new InvalidOperationException("Open FC26 data before saving a CM26 project session.");
        var project = new ProjectSession
        {
            Version = 1,
            SourceKind = string.IsNullOrWhiteSpace(Fc26SnapshotLoader.CurrentGameRoot) ? "extracted" : "installed",
            GameRoot = Fc26SnapshotLoader.CurrentGameRoot,
            DatabaseFolder = Fc26SnapshotLoader.CurrentDatabaseFolder,
            SavedUtc = DateTime.UtcNow
        };
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(fileName))!);
        File.WriteAllText(fileName, JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true }));
        AddRecent(fileName);
        Fc26ActivityLog.Add("Project", "Saved CM26 session: " + fileName);
    }

    internal static ProjectSession Load(string fileName)
    {
        if (!File.Exists(fileName)) throw new FileNotFoundException("CM26 project session was not found.", fileName);
        var project = JsonSerializer.Deserialize<ProjectSession>(File.ReadAllText(fileName),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("CM26 project session is empty.");
        if (project.Version != 1) throw new InvalidDataException("Unsupported CM26 project session version: " + project.Version);
        AddRecent(fileName);
        return project;
    }

    internal static IReadOnlyList<string> Recent() => ReadRecent()
        .Where(File.Exists).Take(10).ToArray();

    internal static void AddRecent(string fileName)
    {
        var fullPath = Path.GetFullPath(fileName);
        var recent = ReadRecent().Where(path => !path.Equals(fullPath, StringComparison.OrdinalIgnoreCase)).ToList();
        recent.Insert(0, fullPath);
        recent = recent.Take(10).ToList();
        Directory.CreateDirectory(Path.GetDirectoryName(RecentPath)!);
        File.WriteAllText(RecentPath, JsonSerializer.Serialize(recent));
    }

    private static List<string> ReadRecent()
    {
        try
        {
            if (!File.Exists(RecentPath)) return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RecentPath)) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    internal sealed class ProjectSession
    {
        public int Version { get; set; }
        public string SourceKind { get; set; } = string.Empty;
        public string GameRoot { get; set; } = string.Empty;
        public string DatabaseFolder { get; set; } = string.Empty;
        public DateTime SavedUtc { get; set; }
    }
}
