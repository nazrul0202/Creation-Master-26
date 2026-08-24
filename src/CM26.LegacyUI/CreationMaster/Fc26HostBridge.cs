using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CreationMaster;

internal static class Fc26HostBridge
{
    private const int AssetCommandTimeoutMs = 120000;
    private const int DatabaseCommandTimeoutMs = 300000;
    private static string? s_HostPath;
    private static bool? s_DotnetHostAvailable;
    private static readonly object s_AssetGate = new();
    private static readonly System.Collections.Generic.Dictionary<string, string?> s_AssetCache =
        new(StringComparer.OrdinalIgnoreCase);

    internal static void Configure(string[] args)
    {
        for (var i = 0; i + 1 < args.Length; i++)
        {
            if (string.Equals(args[i], "--cm26-host", StringComparison.OrdinalIgnoreCase))
            {
                s_HostPath = Path.GetFullPath(args[i + 1]);
                return;
            }
        }
    }

    internal static string Open()
    {
        if (string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath))
            throw new FileNotFoundException("CM26 FC26 host executable was not found.", s_HostPath);

        var outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "legacy");
        Directory.CreateDirectory(outputDirectory);
        var snapshotPath = Path.Combine(outputDirectory, "fc26-snapshot.json");

        var start = new ProcessStartInfo
        {
            FileName = s_HostPath,
            Arguments = "--legacy-open \"" + snapshotPath + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
        };
        var result = RunProcess(start, DatabaseCommandTimeoutMs, "FC26 database loader");
        if (result.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.StandardError)
                ? (string.IsNullOrWhiteSpace(result.StandardOutput) ? "FC26 loader failed." : result.StandardOutput)
                : result.StandardError);
        if (!File.Exists(snapshotPath))
            throw new FileNotFoundException("FC26 loader did not create the database snapshot.", snapshotPath);

        return snapshotPath;
    }

    internal static string? ExportAsset(string logicalPath)
    {
        if (string.IsNullOrWhiteSpace(logicalPath) || string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath))
            return null;
        lock (s_AssetGate)
        {
            if (s_AssetCache.TryGetValue(logicalPath, out var cached)) return cached;
            var normalized = logicalPath.Replace('\\', '/').TrimStart('/');
            var diskCache = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Creation Master 26", "legacy-assets-v2",
                normalized.Replace('/', Path.DirectorySeparatorChar));
            var cachedFile = FindCachedAsset(diskCache);
            if (cachedFile != null)
            {
                s_AssetCache[logicalPath] = cachedFile;
                return cachedFile;
            }
            var start = new ProcessStartInfo
            {
                FileName = s_HostPath,
                Arguments = "--legacy-asset \"" + logicalPath.Replace("\"", string.Empty) + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
            };
            try
            {
                var result = RunProcess(start, AssetCommandTimeoutMs, "FC26 asset exporter");
                var output = result.StandardOutput.Trim();
                var resolved = result.ExitCode == 0 && File.Exists(output) ? output : null;
                if (result.ExitCode == 0)
                    s_AssetCache[logicalPath] = resolved ?? string.Empty;
                return resolved;
            }
            catch
            {
                // A timeout or temporary host error is not a confirmed missing asset.
                // Do not poison the cache so the user can retry later.
                return null;
            }
        }
    }

    internal static string? ExportKitTexture(int teamId, int kitType)
    {
        if (string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath)) return null;
        var cacheKey = "kit:" + teamId + ":" + kitType;
        lock (s_AssetGate)
        {
            if (s_AssetCache.TryGetValue(cacheKey, out var known))
                return string.IsNullOrWhiteSpace(known) ? null : known;
            var diskCache = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Creation Master 26", "legacy-kit-textures-v1", teamId + "_" + kitType + ".png");
            if (File.Exists(diskCache) && new FileInfo(diskCache).Length > 0)
            {
                s_AssetCache[cacheKey] = diskCache;
                return diskCache;
            }

            try
            {
                var start = new ProcessStartInfo
                {
                    FileName = s_HostPath,
                    Arguments = "--legacy-kit-texture " + teamId + " " + kitType,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
                };
                var result = RunProcess(start, AssetCommandTimeoutMs, "FC26 kit texture exporter");
                var output = result.StandardOutput.Trim();
                var resolved = result.ExitCode == 0 && File.Exists(output) ? output : null;
                if (result.ExitCode == 0)
                    s_AssetCache[cacheKey] = resolved ?? string.Empty;
                return resolved;
            }
            catch
            {
                return null;
            }
        }
    }

    internal static string StageImage(string legacyPath, string sourcePath, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath))
            throw new FileNotFoundException("CM26 FC26 host executable was not found.", s_HostPath);
        var start = new ProcessStartInfo
        {
            FileName = s_HostPath,
            Arguments = "--legacy-stage-image \"" + legacyPath.Replace("\"", string.Empty) + "\" \"" +
                sourcePath.Replace("\"", string.Empty) + "\" " + width + " " + height,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
        };
        var result = RunProcess(start, AssetCommandTimeoutMs, "FC26 asset importer");
        if (result.ExitCode != 0) throw new InvalidOperationException(result.StandardError.Trim());
        return result.StandardOutput.Trim();
    }

    internal static string ExportFaceMesh(int playerId, int headAssetId)
    {
        if (string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath))
            throw new FileNotFoundException("CM26 FC26 host executable was not found.", s_HostPath);
        var responsePath = Path.Combine(Path.GetTempPath(), "cm26-face-" + Guid.NewGuid().ToString("N") + ".txt");
        var hostDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory;
        var arguments = "--legacy-face-mesh " + playerId + " " + headAssetId + " \"" + responsePath + "\"";
        var hostDll = Path.Combine(hostDirectory, Path.GetFileNameWithoutExtension(s_HostPath) + ".dll");

        // Framework-dependent apphosts can return before a helper command has run when the
        // graphical host is already active. Invoking the sibling assembly through dotnet keeps
        // this legacy x86 -> Frostbite x64 endpoint deterministic. Full self-contained packages
        // fall back to their apphost because they deliberately do not require a system dotnet.
        var useDotnetHost = File.Exists(hostDll) && DotnetHostIsAvailable();
        var start = new ProcessStartInfo
        {
            FileName = useDotnetHost ? "dotnet" : s_HostPath,
            Arguments = useDotnetHost ? "\"" + hostDll + "\" " + arguments : arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = hostDirectory
        };
        try
        {
            var result = RunProcess(start, AssetCommandTimeoutMs, "FC26 face mesh exporter");
            var output = File.Exists(responsePath) ? File.ReadAllText(responsePath).Trim() : string.Empty;
            if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(output) || !File.Exists(output))
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.StandardError)
                    ? "The selected player has no indexed FC26 Frostbite head mesh."
                    : result.StandardError.Trim());
            return output;
        }
        finally
        {
            try { if (File.Exists(responsePath)) File.Delete(responsePath); } catch { }
        }
    }

    internal static EquipmentPreview ExportEquipmentPreview(string kind, int id)
    {
        if (string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath))
            throw new FileNotFoundException("CM26 FC26 host executable was not found.", s_HostPath);
        var response = Path.Combine(Path.GetTempPath(), "cm26-equipment-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            var start = new ProcessStartInfo
            {
                FileName = s_HostPath,
                Arguments = "--legacy-equipment-preview " + kind + " " + id + " \"" + response + "\"",
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
            };
            var result = RunProcess(start, AssetCommandTimeoutMs, "FC26 equipment preview exporter");
            if (result.ExitCode != 0) throw new InvalidOperationException(result.StandardError.Trim());
            var lines = File.Exists(response) ? File.ReadAllLines(response) : Array.Empty<string>();
            if (lines.Length == 0 || !File.Exists(lines[0])) throw new FileNotFoundException("Equipment mesh was not exported.");
            return new EquipmentPreview(lines[0], lines.Length > 1 && File.Exists(lines[1]) ? lines[1] : null);
        }
        finally { try { if (File.Exists(response)) File.Delete(response); } catch { } }
    }

    internal sealed class EquipmentPreview
    {
        internal EquipmentPreview(string meshPath, string? texturePath) { MeshPath = meshPath; TexturePath = texturePath; }
        internal string MeshPath { get; }
        internal string? TexturePath { get; }
    }

    internal static ScoreboardAsset[] LoadScoreboards()
    {
        if (string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath)) return Array.Empty<ScoreboardAsset>();
        var response = Path.Combine(Path.GetTempPath(), "cm26-scoreboards-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            var start = new ProcessStartInfo
            {
                FileName = s_HostPath, Arguments = "--legacy-scoreboards \"" + response + "\"",
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
            };
            var result = RunProcess(start, AssetCommandTimeoutMs, "FC26 scoreboard catalog");
            if (result.ExitCode != 0) throw new InvalidOperationException(result.StandardError.Trim());
            return File.Exists(response) ? File.ReadAllLines(response).Select(line => line.Split(new[] { '\t' }, 2))
                .Where(parts => parts.Length == 2).Select(parts => new ScoreboardAsset(parts[0], parts[1])).ToArray()
                : Array.Empty<ScoreboardAsset>();
        }
        finally { try { if (File.Exists(response)) File.Delete(response); } catch { } }
    }

    internal sealed class ScoreboardAsset
    {
        internal ScoreboardAsset(string type, string name) { Type = type; Name = name; }
        internal string Type { get; }
        internal string Name { get; }
        public override string ToString() => Path.GetFileName(Name.Replace('/', Path.DirectorySeparatorChar));
    }

    private static bool DotnetHostIsAvailable()
    {
        if (s_DotnetHostAvailable.HasValue) return s_DotnetHostAvailable.Value;
        try
        {
            var result = RunProcess(new ProcessStartInfo("dotnet", "--info")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }, 5000, ".NET host check");
            s_DotnetHostAvailable = result.ExitCode == 0;
        }
        catch
        {
            s_DotnetHostAvailable = false;
        }
        return s_DotnetHostAvailable.Value;
    }

    internal static void OpenFaceViewer(string meshPath)
    {
        if (string.IsNullOrWhiteSpace(s_HostPath))
            throw new InvalidOperationException("CM26 FC26 host is not configured.");
        var viewer = Path.Combine(Path.GetDirectoryName(s_HostPath) ?? string.Empty,
            "Tools", "CM26.3DViewer", "3D Face Viewer By Rizco98 FET Renderer.exe");
        if (!File.Exists(viewer))
            throw new FileNotFoundException("The packaged CM26 3D viewer is unavailable.", viewer);
        var start = new ProcessStartInfo(viewer)
        {
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(viewer) ?? Environment.CurrentDirectory,
            Arguments = "\"" + meshPath.Replace("\"", string.Empty) + "\""
        };
        Process.Start(start);
    }

    internal static void PreloadAssets(IEnumerable<string> logicalPaths)
    {
        if (logicalPaths == null || string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath)) return;
        lock (s_AssetGate)
        {
            // A rapid section/league change may queue several background loads.
            // Re-check inside one process gate so only one FC26 archive session runs
            // at a time and later requests benefit from the cache it populated.
            var missing = logicalPaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(path =>
                {
                    if (s_AssetCache.ContainsKey(path)) return false;
                    var normalized = path.Replace('\\', '/').TrimStart('/');
                    var diskCache = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Creation Master 26", "legacy-assets-v2",
                        normalized.Replace('/', Path.DirectorySeparatorChar));
                    return FindCachedAsset(diskCache) == null;
                })
                .ToArray();
            if (missing.Length == 0) return;

            var request = Path.Combine(Path.GetTempPath(), "cm26-assets-" + Guid.NewGuid().ToString("N") + ".txt");
            try
            {
                File.WriteAllLines(request, missing);
                var start = new ProcessStartInfo
                {
                    FileName = s_HostPath,
                    Arguments = "--legacy-assets-list \"" + request.Replace("\"", string.Empty) + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
                };
                using var process = Process.Start(start);
                if (process == null) return;
                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();
                var completed = process.WaitForExit(30000);
                if (!completed)
                {
                    try { process.Kill(); } catch { }
                }
                System.Threading.Tasks.Task.WaitAll(new System.Threading.Tasks.Task[] { output, error }, 5000);
                var canCacheMisses = completed && process.ExitCode == 0;

                // Record confirmed hits and, after a successful host run, confirmed
                // misses. This prevents optional missing crests from relaunching the
                // x64 host on every list refresh without hiding transient host errors.
                foreach (var path in missing)
                {
                    var normalized = path.Replace('\\', '/').TrimStart('/');
                    var diskCache = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Creation Master 26", "legacy-assets-v2",
                        normalized.Replace('/', Path.DirectorySeparatorChar));
                    var cachedAsset = FindCachedAsset(diskCache);
                    if (cachedAsset != null || canCacheMisses)
                        s_AssetCache[path] = cachedAsset ?? string.Empty;
                }
            }
            catch
            {
                // Individual ExportAsset calls remain the safe fallback.
            }
            finally
            {
                try { File.Delete(request); } catch { }
            }
        }
    }

    private static string? FindCachedAsset(string requestedPath)
    {
        if (File.Exists(requestedPath) && new FileInfo(requestedPath).Length > 0) return requestedPath;
        var directory = Path.GetDirectoryName(requestedPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return null;
        var stem = Path.GetFileNameWithoutExtension(requestedPath);
        foreach (var extension in new[] { ".png", ".jpg", ".jpeg", ".bmp", ".dds" })
        {
            var candidate = Path.Combine(directory, stem + extension);
            if (File.Exists(candidate) && new FileInfo(candidate).Length > 0) return candidate;
        }
        return null;
    }

    internal static string Save()
    {
        if (string.IsNullOrWhiteSpace(s_HostPath) || !File.Exists(s_HostPath))
            throw new FileNotFoundException("CM26 FC26 host executable was not found.", s_HostPath);
        var outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "legacy");
        Directory.CreateDirectory(outputDirectory);
        var planPath = Path.Combine(outputDirectory, "fc26-changes.json");
        var changeCount = Fc26SnapshotLoader.WriteChanges(planPath);
        var start = new ProcessStartInfo
        {
            FileName = s_HostPath,
            Arguments = "--legacy-save \"" + planPath + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(s_HostPath) ?? Environment.CurrentDirectory
        };
        var result = RunProcess(start, DatabaseCommandTimeoutMs, "FC26 save engine");
        var output = result.StandardOutput.Trim();
        var error = result.StandardError.Trim();
        if (result.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? output : error);
        return string.IsNullOrWhiteSpace(output) ? $"Saved {changeCount} FC26 change(s)." : output;
    }

    private static ProcessResult RunProcess(ProcessStartInfo start, int timeoutMilliseconds, string operation)
    {
        using var process = Process.Start(start) ??
            throw new InvalidOperationException("Unable to start the " + operation + ".");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(timeoutMilliseconds))
        {
            try { process.Kill(); } catch { }
            try { System.Threading.Tasks.Task.WaitAll(new System.Threading.Tasks.Task[] { outputTask, errorTask }, 5000); } catch { }
            throw new TimeoutException(operation + " did not finish within " + (timeoutMilliseconds / 1000) + " seconds.");
        }
        try { System.Threading.Tasks.Task.WaitAll(new System.Threading.Tasks.Task[] { outputTask, errorTask }, 5000); } catch { }
        return new ProcessResult(process.ExitCode,
            outputTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion ? outputTask.Result : string.Empty,
            errorTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion ? errorTask.Result : string.Empty);
    }

    private sealed class ProcessResult
    {
        internal ProcessResult(int exitCode, string standardOutput, string standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput ?? string.Empty;
            StandardError = standardError ?? string.Empty;
        }

        internal int ExitCode { get; }
        internal string StandardOutput { get; }
        internal string StandardError { get; }
    }
}
