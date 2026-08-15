namespace CM26.AssetBridge;

/// <summary>
/// In-process entry point for the FC26 Frostbite asset engine. Runs in the
/// same process as the application (single EXE, like CM16's FifaLibrary) and
/// keeps the indexed inventory cached until the game root changes.
/// </summary>
public static class BridgeEngine
{
    private static readonly object IndexGate = new();
    private static string _indexedRoot = string.Empty;
    private static FrostbiteInventory? _indexedInventory;

    /// <summary>Opens the game root and (re)indexes the Frostbite archives.</summary>
    public static FrostbiteInventory OpenGame(string gameRoot)
    {
        var inventory = EnsureIndexed(gameRoot);
        return inventory;
    }

    public static IReadOnlyList<BridgeAssetResult> SearchAssets(
        string gameRoot, string query, string? assetType, int maximum = 100)
    {
        _ = EnsureIndexed(gameRoot);
        var kind = ParseKind(assetType);
        return FrostbiteAssetIndexStore.Search(query, kind, maximum)
            .Select(ToBridgeAsset)
            .ToArray();
    }

    /// <summary>Extracts an exact asset to a temp file and returns its path.</summary>
    public static string ExtractAsset(string gameRoot, string assetType, string query)
    {
        _ = EnsureIndexed(gameRoot);
        var kind = ParseKind(assetType)
            ?? throw new ArgumentException("Asset type is required for exact extraction.");
        var asset = FrostbiteAssetIndexStore.FindExact(query, kind)
            ?? throw new FileNotFoundException($"Asset was not found: {assetType}:{query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(gameRoot, "Patch", "layout.toc"));
        return FrostbitePayloadReader.Extract(gameRoot, layout.Catalogs, asset);
    }

    /// <summary>Parses an FC26 MeshSet RES and exports self-contained ASCII FBX.</summary>
    public static string ExportMesh(string gameRoot, string query, string? textureToken = null)
    {
        _ = EnsureIndexed(gameRoot);
        var resource = FrostbiteAssetIndexStore.FindExact(query, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"Mesh RES was not found: {query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(gameRoot, "Patch", "layout.toc"));
        var payload = FrostbitePayloadReader.ReadDecoded(gameRoot, layout.Catalogs, resource);
        var meta = Convert.FromHexString(resource.ResMeta);
        using var stream = new MemoryStream(payload, writable: false);

        var mesh = new CM26.MeshKit.MeshSet(CM26.MeshKit.DataVersion.FC26, stream, meta);
        var exporter = new CM26.MeshKit.MeshFbxExporter(mesh);

        foreach (var lod in mesh.Lods)
        {
            if (lod.ChunkId == Guid.Empty) continue;
            var chunk = FrostbiteAssetIndexStore.FindExact(
                lod.ChunkId.ToString("D"), FrostbiteAssetKind.Chunk)
                ?? throw new FileNotFoundException($"LOD chunk was not indexed: {lod.ChunkId}");
            var chunkPayload = FrostbitePayloadReader.ReadDecoded(gameRoot, layout.Catalogs, chunk);
            exporter.AddChunkData(lod.ChunkId, chunkPayload);
        }

        var outputRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "exported-meshes");
        Directory.CreateDirectory(outputRoot);
        var safeName = string.IsNullOrWhiteSpace(resource.Sha1)
            ? resource.Name.Replace('-', '_').Replace('/', '_').Replace('\\', '_')
            : resource.Sha1;

        // Best-effort loose texture lookup reusing the record token used to
        // pick the mesh. EMBAD-texture is optional; geometry/skin export never
        // depends on it.
        string? textureFileName = null;
        try
        {
            textureFileName = ResolveSidecarTexture(gameRoot, layout.Catalogs, textureToken, outputRoot, safeName);
        }
        catch
        {
            // Best-effort: a missing texture must never fail the mesh export.
        }

        var fbx = exporter.Export(resource.Name, textureFileName);

        var destination = Path.Combine(outputRoot, safeName + ".fbx");
        File.WriteAllText(destination, fbx);
        return destination;
    }

    /// <summary>Inspects the geometry declaration of a MeshSet RES without exporting.</summary>
    public static string MeshDecl(string gameRoot, string query)
    {
        _ = EnsureIndexed(gameRoot);
        var resource = FrostbiteAssetIndexStore.FindExact(query, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"Mesh RES was not found: {query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(gameRoot, "Patch", "layout.toc"));
        var payload = FrostbitePayloadReader.ReadDecoded(gameRoot, layout.Catalogs, resource);
        var meta = Convert.FromHexString(resource.ResMeta);
        using var stream = new MemoryStream(payload, writable: false);
        var mesh = new CM26.MeshKit.MeshSet(CM26.MeshKit.DataVersion.FC26, stream, meta);
        var report = new System.Text.StringBuilder();
        report.Append($"type={mesh.Type} parts={mesh.PartCount} bones={mesh.BoneIndices?.Length ?? 0}");
        foreach (var lod in mesh.Lods)
        {
            report.AppendLine();
            report.Append($"LOD '{lod.ShortName}' type={lod.Type} sections={lod.Sections.Count} boneIndexArray={lod.BoneIndexArray.Count} inline={lod.InlineData?.Length ?? 0}B");
            foreach (var section in lod.Sections)
            {
                if (section.GeometryDeclDesc.Length == 0) continue;
                report.AppendLine();
                report.Append($"  SEC '{section.Name}' verts={section.VertexCount} bonesPerVert={section.BonesPerVertex} boneList={section.BoneList.Length}");
                var decl = section.GeometryDeclDesc[0];
                report.Append($"  decl streams={decl.StreamCount} elements={decl.ElementCount}");
                for (var i = 0; i < decl.ElementCount; i++)
                {
                    var element = decl.Elements[i];
                    report.Append($" [{element.Usage}/{element.Format}@s{element.StreamIndex}+{element.Offset}]");
                }
                report.Append($"  streams=");
                for (var s = 0; s < decl.StreamCount; s++)
                    report.Append($"(stride {decl.Streams[s].VertexStride})");
            }
        }
        return report.ToString();
    }

    /// <summary>Exports a texture RES to a DDS temp file and returns its path.</summary>
    public static string ExportTexture(string gameRoot, string query)
    {
        _ = EnsureIndexed(gameRoot);
        var resource = FrostbiteAssetIndexStore.FindExact(query, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"Texture RES was not found: {query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(gameRoot, "Patch", "layout.toc"));
        return FrostbiteTextureExporter.ExportDds(gameRoot, layout.Catalogs, resource);
    }

    /// <summary>Exports a named legacy UI file through FC26's own collector.</summary>
    public static string ExportLegacy(string gameRoot, string query)
    {
        _ = EnsureIndexed(gameRoot);
        var layout = FrostbiteLayoutReader.Read(Path.Combine(gameRoot, "Patch", "layout.toc"));
        return FrostbiteLegacyAssetResolver.Export(gameRoot, layout.Catalogs, query);
    }

    /// <summary>Parses a NewWave sample bank RES and returns the bank + extracted path.</summary>
    public static (AudioBankResult Bank, string ExtractedPath) InspectAudio(string gameRoot, string query)
    {
        _ = EnsureIndexed(gameRoot);
        var asset = FrostbiteAssetIndexStore.FindExact(query, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"NewWave RES was not found: {query}");
        if (asset.ResType != 2999215606)
            throw new InvalidDataException(
                $"RES type 0x{asset.ResType:X8} is not an FC26 NewWave sample bank.");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(gameRoot, "Patch", "layout.toc"));
        var path = FrostbitePayloadReader.Extract(gameRoot, layout.Catalogs, asset);
        var bank = NewWaveAudioInspector.Inspect(asset.Name, File.ReadAllBytes(path));
        return (bank, path);
    }

    /// <summary>Applies a validated direct-edit plan to the game Data/Patch tree.</summary>
    public static string ApplyDirect(string gameRoot, string planPath)
    {
        var inventory = EnsureIndexed(gameRoot);
        var result = FrostbiteDirectLegacyWriter.Apply(inventory.GameRoot, planPath);
        lock (IndexGate)
        {
            _indexedRoot = string.Empty;
            _indexedInventory = null;
        }
        return FormatApplyMessage(result, verified: false);
    }

    public static string VerifyDirect(string gameRoot, string planPath)
    {
        var inventory = EnsureIndexed(gameRoot);
        var result = FrostbiteDirectLegacyWriter.Verify(inventory.GameRoot, planPath);
        return FormatApplyMessage(result, verified: true);
    }

    /// <summary>Exports a FET mod folder from a direct-edit plan.</summary>
    public static string ExportFet(string gameRoot, string planPath, string output)
    {
        _ = EnsureIndexed(gameRoot);
        var result = FrostbiteDirectLegacyWriter.ExportFetMod(gameRoot, planPath, output);
        return $"FET mod exported: {result.Applied} legacy edit(s), {result.Skipped.Count} skipped.";
    }

    private static string FormatApplyMessage(FrostbiteDirectLegacyWriter.ApplyResult result, bool verified)
    {
        var verb = verified ? "Verified" : "Applied";
        var applied = verified
            ? $" {result.Applied:N0} direct FC26 database/legacy replacement(s) without changing Data/Patch."
            : $" {result.Applied:N0} database/legacy replacement(s) directly to FC26 Data/Patch.";
        var cache = !verified && result.ClearedGameCaches > 0
            ? $" Cleared {result.ClearedGameCaches:N0} stale FC26 Assets cache file(s)."
            : string.Empty;
        if (result.Skipped.Count == 0) return verb + applied + cache;
        return verb + applied + cache +
            $" Skipped {result.Skipped.Count:N0} staged file(s) not present in this installation: " +
            string.Join(", ", result.Skipped) + ".";
    }

    /// <summary>
    /// Resolves a loose <c>*_color</c> texture beside the exported FBX. The
    /// mesh RES has no material-to-texture link, so the exporter guesses from
    /// the same record token used to find the mesh: the first indexed texture
    /// whose name contains the token is exported to the mesh output folder and
    /// the relative file name is embedded in the FBX material for viewers that
    /// read FBX materials. Returns null when no loose texture matched.
    /// </summary>
    private static string? ResolveSidecarTexture(
        string gameRoot,
        IReadOnlyDictionary<uint, string> catalogs,
        string? token,
        string meshOutputDirectory,
        string meshSafeName)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var texture = FrostbiteAssetIndexStore.Search(token, FrostbiteAssetKind.Res, 200)
            .Where(a => a.ResType == CM26.AssetBridge.FrostbiteTextureExporter.TextureResType)
            .OrderByDescending(a => a.Name.EndsWith("_color", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenByDescending(a => a.Name.Contains(token, StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .FirstOrDefault();
        if (texture == null) return null;

        var source = FrostbiteTextureExporter.ExportDds(gameRoot, catalogs, texture);
        var fileName = Path.GetFileName(source);
        var destination = Path.Combine(meshOutputDirectory, meshSafeName + "_" + fileName);
        if (!File.Exists(destination))
            File.Copy(source, destination, overwrite: false);
        return Path.GetFileName(destination);
    }

    private static FrostbiteAssetKind? ParseKind(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Enum.TryParse<FrostbiteAssetKind>(value, ignoreCase: true, out var kind)
            ? kind
            : throw new ArgumentException($"Unknown asset type: {value}");
    }

    private static FrostbiteInventory EnsureIndexed(string root)
    {
        root = Path.GetFullPath(root);
        lock (IndexGate)
        {
            if (_indexedInventory != null && string.Equals(_indexedRoot, root, StringComparison.OrdinalIgnoreCase))
                return _indexedInventory;
            _indexedInventory = new FrostbiteContainerScanner().Scan(root);
            _indexedRoot = root;
            return _indexedInventory;
        }
    }

    private static BridgeAssetResult ToBridgeAsset(FrostbiteIndexedAsset asset) =>
        new(asset.Kind.ToString().ToUpperInvariant(), asset.Name, asset.Sha1,
            asset.OriginalSize, asset.Size, asset.ResType, asset.ResRid);
}