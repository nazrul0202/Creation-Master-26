using System;
using System.Diagnostics;
using System.IO;

namespace CreationMaster;

internal static class Fc26HostBridge
{
    private static string? s_HostPath;
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
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Unable to start the FC26 loader.");
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(standardError)
                ? (string.IsNullOrWhiteSpace(standardOutput) ? "FC26 loader failed." : standardOutput)
                : standardError);
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
                using var process = Process.Start(start);
                if (process == null) return null;
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.StandardError.ReadToEnd();
                process.WaitForExit();
                var resolved = process.ExitCode == 0 && File.Exists(output) ? output : null;
                s_AssetCache[logicalPath] = resolved;
                return resolved;
            }
            catch
            {
                s_AssetCache[logicalPath] = null;
                return null;
            }
        }
    }
}
