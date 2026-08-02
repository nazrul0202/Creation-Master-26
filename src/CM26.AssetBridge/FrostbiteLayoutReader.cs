namespace CM26.AssetBridge;

internal sealed record FrostbiteLayoutInfo(
    int Base,
    int Head,
    int SuperBundleCount,
    int CatalogCount,
    IReadOnlyDictionary<uint, string> Catalogs);

internal static class FrostbiteLayoutReader
{
    public static FrostbiteLayoutInfo Read(string layoutPath)
    {
        var root = new FrostbiteDbReader().ReadObject(layoutPath);
        var baseVersion = GetInt(root, "base");
        var headVersion = GetInt(root, "head");
        var superBundleCount = GetList(root, "superBundles").Count;

        var catalogs = new Dictionary<uint, string>();
        if (root.TryGetValue("installManifest", out var manifestValue) &&
            manifestValue is Dictionary<string, object?> manifest)
            foreach (var value in GetList(manifest, "installChunks"))
            {
                if (value is not Dictionary<string, object?> chunk ||
                    !chunk.TryGetValue("persistentIndex", out var indexValue) ||
                    indexValue is not int index ||
                    !chunk.TryGetValue("name", out var nameValue) ||
                    nameValue is not string name)
                    continue;
                catalogs[unchecked((uint)index)] = name;
            }

        return new FrostbiteLayoutInfo(
            baseVersion, headVersion, superBundleCount, catalogs.Count, catalogs);
    }

    private static int GetInt(Dictionary<string, object?> source, string key) =>
        source.TryGetValue(key, out var value) && value is int number ? number : 0;

    private static List<object?> GetList(Dictionary<string, object?> source, string key) =>
        source.TryGetValue(key, out var value) && value is List<object?> list ? list : [];
}
