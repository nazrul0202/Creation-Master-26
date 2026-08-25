using System.IO.Compression;
using System.Text.Json;

namespace CM26.App;

/// <summary>
/// CM26's standalone, portable mod format. A package stores only the edited
/// payloads, never a copy of the game. The Mod Manager will materialise those
/// payloads into its own CM26ModData overlay at launch time.
/// </summary>
public static class CM26ModPackageService
{
    public const string Extension = ".cm26mod";
    private const string ManifestEntry = "manifest.json";

    public sealed record Payload(string GamePath, string SourcePath);
    public sealed record PackageManifest(int FormatVersion, string Name, string Game,
        DateTimeOffset CreatedUtc, PackagePayload[] Payloads);
    public sealed record PackagePayload(string GamePath, string EntryName, long Length, string Sha256);

    public static PackageManifest Export(string destination, string name, IEnumerable<Payload> payloads)
        => Export(destination, name, payloads, Extension);

    private static PackageManifest Export(string destination, string name, IEnumerable<Payload> payloads, string extension)
    {
        if (!destination.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            destination += extension;
        var items = payloads
            .Select(item => new Payload(NormalizeGamePath(item.GamePath), item.SourcePath))
            .OrderBy(item => item.GamePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (items.Length == 0) throw new InvalidOperationException("No CM26 changes are staged for export.");
        if (items.Any(item => !File.Exists(item.SourcePath)))
            throw new FileNotFoundException("A staged CM26 payload is missing.");

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
        var temporary = destination + ".tmp";
        try
        {
            using var archive = ZipFile.Open(temporary, ZipArchiveMode.Create);
            var manifestPayloads = new List<PackagePayload>();
            foreach (var item in items)
            {
                var entryName = "payload/" + item.GamePath;
                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                using (var input = File.OpenRead(item.SourcePath))
                using (var output = entry.Open()) input.CopyTo(output);
                manifestPayloads.Add(new(item.GamePath, entryName, new FileInfo(item.SourcePath).Length, Hash(item.SourcePath)));
            }
            var manifest = new PackageManifest(1, name.Trim(), "EA SPORTS FC 26", DateTimeOffset.UtcNow,
                manifestPayloads.ToArray());
            var manifestEntry = archive.CreateEntry(ManifestEntry, CompressionLevel.Optimal);
            using var writer = new StreamWriter(manifestEntry.Open());
            writer.Write(JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            throw;
        }
        File.Move(temporary, destination, overwrite: true);
        return ReadManifest(destination);
    }

    public static PackageManifest ReadManifest(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.GetEntry(ManifestEntry) ?? throw new InvalidDataException("CM26 mod manifest is missing.");
        using var reader = new StreamReader(entry.Open());
        var manifest = JsonSerializer.Deserialize<PackageManifest>(reader.ReadToEnd())
            ?? throw new InvalidDataException("CM26 mod manifest is invalid.");
        if (manifest.FormatVersion != 1 || !string.Equals(manifest.Game, "EA SPORTS FC 26", StringComparison.Ordinal))
            throw new InvalidDataException("This package is not a supported FC26 CM26 mod.");
        return manifest;
    }

    public static void ExtractToOverlay(string packagePath, string overlayRoot)
    {
        ExtractToDirectory(packagePath, overlayRoot);
    }

    /// <summary>
    /// Extracts verified payloads under a CM26-owned directory. Used when an
    /// editable project is imported; the caller can then load its database
    /// without ever writing it into FC26 Data/Patch.
    /// </summary>
    public static PackageManifest ExtractToDirectory(string packagePath, string overlayRoot)
    {
        var manifest = ReadManifest(packagePath);
        using var archive = ZipFile.OpenRead(packagePath);
        foreach (var payload in manifest.Payloads)
        {
            var entry = archive.GetEntry(payload.EntryName) ?? throw new InvalidDataException("CM26 mod payload is missing: " + payload.GamePath);
            var target = Path.Combine(overlayRoot, payload.GamePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureChild(overlayRoot, target);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            entry.ExtractToFile(target, overwrite: true);
            if (new FileInfo(target).Length != payload.Length || !string.Equals(Hash(target), payload.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("CM26 mod payload checksum failed: " + payload.GamePath);
        }
        return manifest;
    }

    private static string NormalizeGamePath(string value)
    {
        var normalized = value.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Split('/').Any(part => part is "." or ".."))
            throw new InvalidDataException("Invalid CM26 mod game path.");
        return normalized.ToLowerInvariant();
    }

    private static void EnsureChild(string root, string candidate)
    {
        var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(candidate).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new IOException("CM26 mod target is outside the overlay.");
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
    }
}
