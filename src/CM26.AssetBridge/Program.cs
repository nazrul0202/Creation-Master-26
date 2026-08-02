using System.Text.Json;

namespace CM26.AssetBridge;

internal static class Program
{
    private static readonly object IndexGate = new();
    private static string _indexedRoot = string.Empty;
    private static FrostbiteInventory? _indexedInventory;
    private static int Main(string[] args)
    {
        if (args.Length >= 2 && args[0].Equals("--scan", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest("openGame", args[1]));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--search", StringComparison.OrdinalIgnoreCase))
        {
            var maximum = args.Length >= 5 && int.TryParse(args[4], out var parsedMaximum)
                ? Math.Clamp(parsedMaximum, 1, 500)
                : 100;
            var response = Execute(new BridgeRequest(
                "searchAssets", args[1], args[2],
                args.Length >= 4 ? args[3] : null,
                MaxResults: maximum));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 4 && args[0].Equals("--extract", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest(
                "extractAsset", args[1], args[3], args[2]));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--mesh", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest(
                "exportMesh", args[1], args[2], "Res",
                args.Length >= 4 ? args[3] : null));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--mesh-decl", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest(
                "meshDecl", args[1], args[2], "Res"));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--texture", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest(
                "exportTexture", args[1], args[2], "Res"));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--audio-bank", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest("inspectAudio", args[1], args[2], "Res"));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--legacy", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest("exportLegacy", args[1], args[2]));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--apply-direct", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest(
                "applyDirect", args[1], args[2]));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }
        if (args.Length >= 3 && args[0].Equals("--verify-direct", StringComparison.OrdinalIgnoreCase))
        {
            var response = Execute(new BridgeRequest(
                "verifyDirect", args[1], args[2]));
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            return response.Ok ? 0 : 2;
        }

        // Long-lived newline-delimited JSON protocol. One response is emitted
        // for every request, which keeps the WinForms process isolated from all
        // future binary parsing and texture conversion faults.
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            BridgeResponse response;
            try
            {
                var request = JsonSerializer.Deserialize<BridgeRequest>(line, BridgeJson.Options)
                    ?? throw new InvalidDataException("Empty bridge request.");
                response = Execute(request);
            }
            catch (Exception ex)
            {
                response = new BridgeResponse(false, "invalid", ex.Message);
            }
            Console.WriteLine(JsonSerializer.Serialize(response, BridgeJson.Options));
            Console.Out.Flush();
        }
        return 0;
    }

    private static BridgeResponse Execute(BridgeRequest request)
    {
        try
        {
            return request.Command.ToLowerInvariant() switch
            {
                "ping" => new BridgeResponse(true, request.Command, "CM26 Asset Bridge ready"),
                "opengame" or "scan" => OpenGame(request),
                "searchassets" => SearchAssets(request),
                "extractasset" => ExtractAsset(request),
                "exportmesh" => ExportMesh(request),
                "meshdecl" => MeshDecl(request),
                "exporttexture" => ExportTexture(request),
                "exportlegacy" => ExportLegacy(request),
                "inspectaudio" => InspectAudio(request),
                "applydirect" => ApplyDirect(request),
                "verifydirect" => VerifyDirect(request),
                _ => new BridgeResponse(false, request.Command, $"Unknown command: {request.Command}"),
            };
        }
        catch (Exception ex)
        {
            return new BridgeResponse(false, request.Command, ex.Message);
        }
    }

    private static BridgeResponse OpenGame(BridgeRequest request)
    {
        var inventory = EnsureIndexed(request.GameRoot ?? string.Empty);
        return new BridgeResponse(
            true, request.Command,
            $"FC26 asset source ready: {inventory.SuperBundleCount} superbundles, " +
            $"{inventory.IndexedBundleCount} bundles, " +
            $"{inventory.UniqueAssetCount} unique assets",
            inventory);
    }

    private static BridgeResponse SearchAssets(BridgeRequest request)
    {
        _ = EnsureIndexed(request.GameRoot ?? string.Empty);
        var kind = ParseKind(request.AssetType);
        var assets = FrostbiteAssetIndexStore.Search(
                request.Query ?? string.Empty, kind, request.MaxResults)
            .Select(ToBridgeAsset)
            .ToArray();
        return new BridgeResponse(
            true, request.Command, $"{assets.Length} asset(s) matched", Assets: assets);
    }

    private static BridgeResponse ExtractAsset(BridgeRequest request)
    {
        var root = request.GameRoot ?? string.Empty;
        _ = EnsureIndexed(root);
        var kind = ParseKind(request.AssetType)
            ?? throw new ArgumentException("Asset type is required for exact extraction.");
        var asset = FrostbiteAssetIndexStore.FindExact(
            request.Query ?? string.Empty, kind)
            ?? throw new FileNotFoundException(
                $"Asset was not found: {request.AssetType}:{request.Query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var output = FrostbitePayloadReader.Extract(root, layout.Catalogs, asset);
        return new BridgeResponse(
            true, request.Command,
            $"Extracted {kind}:{asset.Name} ({asset.OriginalSize} bytes)",
            Assets: [ToBridgeAsset(asset)], OutputPath: output);
    }

    private static BridgeResponse ExportMesh(BridgeRequest request)
    {
        var root = request.GameRoot ?? string.Empty;
        _ = EnsureIndexed(root);
        var resource = FrostbiteAssetIndexStore.FindExact(
            request.Query ?? string.Empty, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"Mesh RES was not found: {request.Query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var payload = FrostbitePayloadReader.ReadDecoded(root, layout.Catalogs, resource);
        var meta = Convert.FromHexString(resource.ResMeta);
        using var stream = new MemoryStream(payload, writable: false);

        var mesh = new CM26.MeshKit.MeshSet(CM26.MeshKit.DataVersion.FC26, stream, meta);
        var exporter = new CM26.MeshKit.MeshFbxExporter(mesh);

        foreach (var lod in mesh.Lods)
        {
            if (lod.ChunkId == Guid.Empty) continue;
            var chunk = FrostbiteAssetIndexStore.FindExact(
                lod.ChunkId.ToString("D"), FrostbiteAssetKind.Chunk)
                ?? throw new FileNotFoundException(
                    $"LOD chunk was not indexed: {lod.ChunkId}");
            var chunkPayload = FrostbitePayloadReader.ReadDecoded(root, layout.Catalogs, chunk);
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
        var textureToken = request.OutputPath;
        string? textureFileName = null;
        try
        {
            textureFileName = ResolveSidecarTexture(root, layout.Catalogs, textureToken, outputRoot, safeName);
        }
        catch
        {
            // Best-effort: a missing texture must never fail the mesh export.
        }

        var fbx = exporter.Export(resource.Name, textureFileName);

        var destination = Path.Combine(outputRoot, safeName + ".fbx");
        File.WriteAllText(destination, fbx);
        return new BridgeResponse(
            true, request.Command,
            $"Exported FC26 mesh: {resource.Name} ({mesh.Lods.Count} LODs)",
            Assets: [ToBridgeAsset(resource)], OutputPath: destination);
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

    private static BridgeResponse ExportTexture(BridgeRequest request)
    {
        var root = request.GameRoot ?? string.Empty;
        _ = EnsureIndexed(root);
        var resource = FrostbiteAssetIndexStore.FindExact(
            request.Query ?? string.Empty, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"Texture RES was not found: {request.Query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var output = FrostbiteTextureExporter.ExportDds(root, layout.Catalogs, resource);
        return new BridgeResponse(
            true, request.Command,
            $"Exported FC26 texture: {resource.Name}",
            Assets: [ToBridgeAsset(resource)], OutputPath: output);
    }

    private static BridgeResponse MeshDecl(BridgeRequest request)
    {
        var root = request.GameRoot ?? string.Empty;
        _ = EnsureIndexed(root);
        var resource = FrostbiteAssetIndexStore.FindExact(
            request.Query ?? string.Empty, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"Mesh RES was not found: {request.Query}");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var payload = FrostbitePayloadReader.ReadDecoded(root, layout.Catalogs, resource);
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
        return new BridgeResponse(
            true, request.Command, report.ToString(),
            Assets: [ToBridgeAsset(resource)]);
    }

    private static BridgeResponse ExportLegacy(BridgeRequest request)
    {
        var root = request.GameRoot ?? string.Empty;
        _ = EnsureIndexed(root);
        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var output = FrostbiteLegacyAssetResolver.Export(root, layout.Catalogs, request.Query ?? string.Empty);
        return new BridgeResponse(true, request.Command, "Exported FC26 legacy UI asset", OutputPath: output);
    }

    private static BridgeResponse InspectAudio(BridgeRequest request)
    {
        var root = request.GameRoot ?? string.Empty;
        _ = EnsureIndexed(root);
        var asset = FrostbiteAssetIndexStore.FindExact(
            request.Query ?? string.Empty, FrostbiteAssetKind.Res)
            ?? throw new FileNotFoundException($"NewWave RES was not found: {request.Query}");
        if (asset.ResType != 2999215606)
            throw new InvalidDataException(
                $"RES type 0x{asset.ResType:X8} is not an FC26 NewWave sample bank.");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var path = FrostbitePayloadReader.Extract(root, layout.Catalogs, asset);
        var bank = NewWaveAudioInspector.Inspect(asset.Name, File.ReadAllBytes(path));
        return new BridgeResponse(
            true, request.Command,
            $"Parsed NewWave bank with {bank.DataSets.Count} dataset(s).",
            Assets: [ToBridgeAsset(asset)], OutputPath: path, AudioBank: bank);
    }

    private static BridgeResponse ApplyDirect(BridgeRequest request)
    {
        var inventory = EnsureIndexed(request.GameRoot ?? string.Empty);
        var count = FrostbiteDirectLegacyWriter.Apply(
            inventory.GameRoot,
            request.Query ?? throw new ArgumentException("Direct-edit plan path is required."));
        lock (IndexGate)
        {
            _indexedRoot = string.Empty;
            _indexedInventory = null;
        }
        return new BridgeResponse(
            true, request.Command,
            $"Applied {count:N0} database/legacy replacement(s) directly to FC26 Data/Patch.");
    }

    private static BridgeResponse VerifyDirect(BridgeRequest request)
    {
        var inventory = EnsureIndexed(request.GameRoot ?? string.Empty);
        var count = FrostbiteDirectLegacyWriter.Verify(
            inventory.GameRoot,
            request.Query ?? throw new ArgumentException("Direct-edit plan path is required."));
        return new BridgeResponse(
            true, request.Command,
            $"Verified {count:N0} direct FC26 database/legacy replacement(s) without changing Data/Patch.");
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
