using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace CM26.App;

/// <summary>
/// Keeps pending asset edits in a CM26-owned workspace until Save commits them
/// through the direct FC26 Data/Patch transaction.
/// </summary>
public sealed class LegacyAssetModService
{
    private static readonly JsonSerializerOptions PlanJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    private readonly Dictionary<string, Replacement> _replacements =
        new(StringComparer.OrdinalIgnoreCase);
    private string _workspace = string.Empty;

    public int Count => _replacements.Count;
    public bool HasChanges => Count > 0;
    public event EventHandler? Changed;

    public void Open(string fingerprint)
    {
        _workspace = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "direct-edit-workspace",
            string.IsNullOrWhiteSpace(fingerprint) ? "fc26" : fingerprint[..Math.Min(16, fingerprint.Length)]);
        Directory.CreateDirectory(Path.Combine(_workspace, "assets"));
        _replacements.Clear();
        var state = Path.Combine(_workspace, "legacy-replacements.json");
        try
        {
            if (File.Exists(state))
            {
                var saved = JsonSerializer.Deserialize<List<Replacement>>(File.ReadAllText(state));
                if (saved != null)
                    foreach (var item in saved.Where(x => File.Exists(x.SourcePath)))
                        _replacements[item.LegacyPath] = item;
            }
        }
        catch { /* A corrupt optional edit state starts empty. */ }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public string? GetReplacement(string legacyPath) =>
        _replacements.TryGetValue(Normalize(legacyPath), out var value) ? value.SourcePath : null;

    public string StageImage(string legacyPath, string sourcePath, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(_workspace))
            throw new InvalidOperationException("Open FC26 before importing assets.");
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Image was not found.", sourcePath);
        var normalized = Normalize(legacyPath);
        var destination = Path.Combine(
            _workspace, "assets", Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(normalized)))[..20] + ".dds");
        if (Path.GetExtension(sourcePath).Equals(".dds", StringComparison.OrdinalIgnoreCase))
            File.Copy(sourcePath, destination, overwrite: true);
        else
            WriteBgraDds(sourcePath, destination, width, height);
        _replacements[normalized] = new Replacement(normalized, destination);
        SaveState();
        Changed?.Invoke(this, EventArgs.Empty);
        return destination;
    }

    public string StageFile(string legacyPath, string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(_workspace))
            throw new InvalidOperationException("Open FC26 before staging legacy files.");
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Legacy file was not found.", sourcePath);
        var normalized = Normalize(legacyPath);
        var extension = Path.GetExtension(sourcePath);
        var destination = Path.Combine(
            _workspace, "assets", Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(normalized)))[..20] +
                (string.IsNullOrWhiteSpace(extension) ? ".bin" : extension.ToLowerInvariant()));
        File.Copy(sourcePath, destination, overwrite: true);
        _replacements[normalized] = new Replacement(normalized, destination);
        SaveState();
        Changed?.Invoke(this, EventArgs.Empty);
        return destination;
    }

    public void StageDatabase(string databaseFolder)
    {
        if (string.IsNullOrWhiteSpace(databaseFolder) || !Directory.Exists(databaseFolder))
            throw new DirectoryNotFoundException("The active FC26 database staging session is unavailable.");
        StageFile("data/db/fifa_ng_db.db", Find(databaseFolder, "fifa_ng_db.db"));
        StageFile("data/loc/eng_us.db", Find(databaseFolder, "eng_us.db"));
    }

    public bool Remove(string legacyPath)
    {
        if (!_replacements.Remove(Normalize(legacyPath))) return false;
        SaveState();
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool MoveReplacement(string sourceLegacyPath, string targetLegacyPath)
    {
        var source = Normalize(sourceLegacyPath);
        var target = Normalize(targetLegacyPath);
        if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
            return _replacements.ContainsKey(source);
        if (!_replacements.Remove(source, out var replacement)) return false;

        _replacements[target] = replacement with { LegacyPath = target };
        SaveState();
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public string WriteDirectPlan()
    {
        if (!HasChanges) throw new InvalidOperationException("No asset replacements are staged.");
        var plan = new
        {
            replacements = _replacements.Values.OrderBy(x => x.LegacyPath).ToArray()
        };
        var path = Path.Combine(_workspace, "cm26-direct-edit-plan.json");
        File.WriteAllText(path, JsonSerializer.Serialize(plan, PlanJson));
        return path;
    }

    public void MarkApplied()
    {
        _replacements.Clear();
        SaveState();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SaveState()
    {
        if (string.IsNullOrWhiteSpace(_workspace)) return;
        Directory.CreateDirectory(_workspace);
        var destination = Path.Combine(_workspace, "legacy-replacements.json");
        var temporary = destination + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(
            _replacements.Values.OrderBy(x => x.LegacyPath).ToArray(),
            new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, destination, overwrite: true);
    }

    private static string Normalize(string value) =>
        value.Replace('\\', '/').TrimStart('/').ToLowerInvariant();

    private static string Find(string folder, string name) =>
        Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(path => Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase))
        ?? throw new FileNotFoundException($"Required staged database file {name} was not found.", name);

    private static void WriteBgraDds(string sourcePath, string destination, int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        using var source = Image.FromFile(sourcePath);
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Transparent);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.DrawImage(source, new Rectangle(0, 0, width, height));
        }
        var data = bitmap.LockBits(
            new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            using var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new BinaryWriter(stream);
            writer.Write("DDS "u8);
            writer.Write(124u);
            writer.Write(0x0002100Fu); // caps, height, width, pitch, pixel format
            writer.Write((uint)height);
            writer.Write((uint)width);
            writer.Write(checked((uint)width * 4));
            writer.Write(0u); // depth
            writer.Write(1u); // mip count
            for (var i = 0; i < 11; i++) writer.Write(0u);
            writer.Write(32u);         // pixel format size
            writer.Write(0x00000041u); // RGB | alpha pixels
            writer.Write(0u);          // fourCC
            writer.Write(32u);
            writer.Write(0x00FF0000u);
            writer.Write(0x0000FF00u);
            writer.Write(0x000000FFu);
            writer.Write(0xFF000000u);
            writer.Write(0x00001000u); // DDSCAPS_TEXTURE
            writer.Write(0u); writer.Write(0u); writer.Write(0u); writer.Write(0u);

            var row = new byte[checked(width * 4)];
            for (var y = 0; y < height; y++)
            {
                Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, row.Length);
                writer.Write(row);
            }
        }
        finally { bitmap.UnlockBits(data); }
    }

    public sealed record Replacement(string LegacyPath, string SourcePath);
}
