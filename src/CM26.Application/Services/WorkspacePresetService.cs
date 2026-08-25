using System.Text.Json;

namespace CM26.Application.Services;

public sealed record WorkspaceFilterPreset(string Name, string TableName, string FieldName, string Expression);

/// <summary>Stores user-created database searches locally; presets never modify a project database.</summary>
public static class WorkspacePresetService
{
    private static readonly string PresetPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "workspace-filters.json");

    public static IReadOnlyList<WorkspaceFilterPreset> Load()
    {
        try
        {
            return File.Exists(PresetPath)
                ? JsonSerializer.Deserialize<List<WorkspaceFilterPreset>>(File.ReadAllText(PresetPath)) ?? []
                : [];
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Workspace filter presets could not be loaded: {ex.Message}");
            return [];
        }
    }

    public static void Save(WorkspaceFilterPreset preset)
    {
        var presets = Load().Where(item => !item.Name.Equals(preset.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        presets.Add(preset);
        Directory.CreateDirectory(Path.GetDirectoryName(PresetPath)!);
        var temporary = PresetPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(presets.OrderBy(item => item.Name),
            new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, PresetPath, overwrite: true);
    }

    public static void Delete(string name)
    {
        var presets = Load().Where(item => !item.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(PresetPath)!);
        File.WriteAllText(PresetPath, JsonSerializer.Serialize(presets, new JsonSerializerOptions { WriteIndented = true }));
    }
}
