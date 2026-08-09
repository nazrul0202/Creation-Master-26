using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json;

namespace CM26.AssetBridge;

/// <summary>
/// Applies legacy-file replacements to the installed FC26 archives without
/// producing a separate mod package. Payloads are prepared and verified first, CAS data is
/// append-only, and TOCs are atomically replaced. Any commit failure restores
/// the previous TOCs and truncates every CAS to its original length.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class FrostbiteDirectLegacyWriter
{
    private const int TocHeaderSize = 556;
    private const int FrostbiteBlockSize = 262144;
    private const int FileEntrySize = 28;
    private const int ManifestHeaderSize = 80;
    private const string TocPrivateKey =
        "UlNBMgAIAAADAAAAAAEAAIAAAACAAAAAAQAB1BUchCbrX19WApHRxhRSPwv9oCUkv3ioR9MXPVmvO4zqbuyL5Jp/PRnoN3qSzltXeqMhWedzNn2MTzXcvw3359ZI1dUvH6Xh5dWQ4Z9MK3dMNEJnQZID3UtG+BiHKeelyT7CdBjVutzjsoacOZ2geaPNfQ+4JIetHdA8qb6EBoGshLGyuWPNQbYojhQmlJQxlzEFTApyh4MAWCuxPlBzH3RjZKJOTA7Hr9Y/NO4C4tPLbsMygKa8tNI3sQBV1LB3U12q9LHRrb5Rh8J0kdopXYvGjSEYtS38us8Oa3bH2lo3b6vufmdGv8MdyW7E4WORb6waIdh6fZJ54i63Tg2dB/JrIQlFziq9cQlUTf2B5WP42lTtDCuhAYhumYpmK3Vm25YjgDnoLuZW8d3GTqrGnLM63GuLzLbg3hBZzkOrDUlcAAHRramtySkzN8hlmuxWDcj+b9wGVliB7aPGtsYlOd7CDTRBN8Rf4shBFxlez2c5Ny+DqX+SqwKrhLB+n/RD3/bj4lgAhJ/cvfP1/AZNBQHs3Dcs/B7oltJ57ZWODaCrtSeywi0bgqBnC6jT3yGVkkaIn6trAhDmJL2llT59KlYNj3BuwHQs9WrZ7INdvH4Py4Hc/7X0OvfEV2Wvh9JDD6+x8phtQff9AjOVxRbgAH+nM3hu0JHPleVKtzQDae0=";
    private static readonly byte[] TocHmacKey =
        System.Text.Encoding.ASCII.GetBytes("Powered by Frostbite \\o/ EA Digital Illusions CE AB");

    private sealed record Plan(IReadOnlyList<Replacement> Replacements);
    private sealed record Replacement(string LegacyPath, string SourcePath);
    private sealed record ResolvedEdit(LegacyAssetTarget Target, byte[] ReplacementBytes);

    /// <summary>
    /// Outcome of a direct-edit preparation. Skipped entries are staged files
    /// whose legacy path is not shipped by this FC26 installation; they are
    /// ignored instead of failing the whole save.
    /// </summary>
    public sealed record ApplyResult(int Applied, IReadOnlyList<string> Skipped, int ClearedGameCaches = 0);
    private sealed record ChunkWrite(Guid Id, FrostbiteCasLocation Original, byte[] Encoded);
    private sealed record TocChunkLocation(string Path, long RecordPosition);
    private sealed class ManifestEntry
    {
        public required ulong Hash { get; init; }
        public required Guid ChunkId { get; init; }
        public required int RowPosition { get; init; }
        public required uint OriginalOffset { get; init; }
        public required uint OriginalSize { get; init; }
        public uint CompressedStartOffset { get; set; }
        public uint CompressedEndOffset { get; set; }
        public uint Offset { get; set; }
        public uint Size { get; set; }
    }

    public static ApplyResult Apply(string gameRoot, string planPath)
        => Prepare(gameRoot, planPath, commit: true);

    public static ApplyResult Verify(string gameRoot, string planPath)
        => Prepare(gameRoot, planPath, commit: false);

    private static ApplyResult Prepare(string gameRoot, string planPath, bool commit)
    {
        var root = Path.GetFullPath(gameRoot);
        EnsureSafeRoot(root);
        EnsureBackupMatchesLiveGame(root);
        var plan = JsonSerializer.Deserialize<Plan>(File.ReadAllText(planPath), BridgeJson.Options)
            ?? throw new InvalidDataException("Direct-edit plan is empty.");
        if (plan.Replacements == null || plan.Replacements.Count == 0)
            throw new InvalidDataException("No FC26 legacy replacements are staged.");

        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var skipped = new List<string>();
        var edits = new List<ResolvedEdit>(plan.Replacements.Count);
        foreach (var item in plan.Replacements)
        {
            if (!File.Exists(item.SourcePath))
                throw new FileNotFoundException("Replacement file was not found.", item.SourcePath);
            LegacyAssetTarget target;
            try
            {
                target = FrostbiteLegacyAssetResolver.ResolveTarget(root, layout.Catalogs, item.LegacyPath);
            }
            catch (FileNotFoundException)
            {
                // The staged path is not shipped by this FC26 installation (for
                // example a league logo staged under a dark/light variant the
                // game does not contain). Skip it gracefully instead of failing
                // the whole save; the caller reports it to the user.
                skipped.Add(item.LegacyPath);
                continue;
            }
            edits.Add(new ResolvedEdit(target, File.ReadAllBytes(item.SourcePath)));
        }
        if (edits.Count == 0)
            throw new InvalidDataException(
                "None of the staged replacements exist in this FC26 installation: " +
                string.Join(", ", skipped));

        var writes = BuildChunkWrites(root, layout.Catalogs, edits);
        var tocLocations = LocateDirectChunks(root, writes.Select(x => x.Id).ToHashSet());
        if (tocLocations.Count != writes.Count)
            throw new InvalidDataException("One or more direct legacy chunks were not found in an FC26 TOC.");
        foreach (var write in writes)
            if (FrostbitePayloadReader.Decompress(write.Encoded, 0, root).Length == 0)
                throw new InvalidDataException($"Prepared chunk {write.Id} did not pass decode verification.");
        if (!commit)
        {
            VerifyTocPlan(root, layout.Catalogs, writes, tocLocations);
            return new ApplyResult(edits.Count, skipped);
        }
        if (new[] { "FC26", "FC26_Trial", "FC26_Showcase" }
            .Any(name => Process.GetProcessesByName(name).Length != 0))
            throw new InvalidOperationException("Close FC26 before applying direct Data/Patch changes.");

        var transaction = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "direct-transactions", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(transaction);
        try
        {
            Commit(root, layout.Catalogs, writes, tocLocations, transaction);
            var clearedCaches = ClearDatabaseAssetCaches();
            return new ApplyResult(edits.Count, skipped, clearedCaches);
        }
        finally
        {
            try { if (Directory.Exists(transaction)) Directory.Delete(transaction, recursive: true); }
            catch { /* A stale transaction folder is safe and can be cleaned later. */ }
        }
    }

    private static List<ChunkWrite> BuildChunkWrites(
        string root, IReadOnlyDictionary<uint, string> catalogs, IReadOnlyList<ResolvedEdit> edits)
    {
        var writes = new List<ChunkWrite>();
        foreach (var collectorGroup in edits.GroupBy(x => x.Target.CollectorManifestChunkId))
        {
            var manifestAsset = FrostbiteAssetIndexStore.FindExact(
                collectorGroup.Key.ToString("D"), FrostbiteAssetKind.Chunk)
                ?? throw new InvalidDataException("ChunkFileCollector manifest was not indexed.");
            var manifest = FrostbitePayloadReader.ReadDecoded(root, catalogs, manifestAsset);
            var entries = ParseManifest(manifest);

            foreach (var chunkGroup in collectorGroup.GroupBy(x => x.Target.OriginalChunkId))
            {
                var first = chunkGroup.First().Target;
                var original = CreateChunkAsset(first);
                var raw = FrostbitePayloadReader.ReadDecoded(root, catalogs, original);
                var relatedEntries = entries.Where(x => x.ChunkId == chunkGroup.Key).ToArray();
                var replacementByHash = chunkGroup.ToDictionary(x => x.Target.NameHash);
                var targets = relatedEntries
                    .Where(x => replacementByHash.ContainsKey(x.Hash))
                    .OrderBy(x => x.OriginalOffset)
                    .ToArray();
                if (targets.Length != replacementByHash.Count)
                    throw new InvalidDataException("A staged legacy entry is missing from its collector manifest.");

                using var rebuilt = new MemoryStream(raw.Length);
                var cursor = 0;
                long cumulativeDelta = 0;
                foreach (var target in targets)
                {
                    var start = checked((int)target.OriginalOffset);
                    var size = checked((int)target.OriginalSize);
                    if (start < cursor || start > raw.Length || size > raw.Length - start)
                        throw new InvalidDataException("Legacy replacement ranges overlap or exceed their source chunk.");
                    rebuilt.Write(raw, cursor, start - cursor);
                    var replacement = replacementByHash[target.Hash].ReplacementBytes;
                    target.Offset = checked((uint)(target.OriginalOffset + cumulativeDelta));
                    target.Size = checked((uint)replacement.Length);
                    rebuilt.Write(replacement);
                    cursor = checked(start + size);
                    cumulativeDelta += replacement.Length - size;
                }
                rebuilt.Write(raw, cursor, raw.Length - cursor);

                foreach (var entry in relatedEntries.Except(targets))
                {
                    long shift = 0;
                    foreach (var target in targets)
                    {
                        var targetEnd = checked((long)target.OriginalOffset + target.OriginalSize);
                        if (entry.OriginalOffset >= targetEnd)
                            shift += checked((long)target.Size - target.OriginalSize);
                        else if (entry.OriginalOffset > target.OriginalOffset)
                            throw new InvalidDataException("A collector entry overlaps a replaced legacy file.");
                    }
                    entry.Offset = checked((uint)(entry.OriginalOffset + shift));
                }

                var encoded = Encode(root, rebuilt.ToArray());
                foreach (var entry in relatedEntries)
                {
                    // A rebuilt chunk is independently block-compressed from
                    // byte zero. Absolute raw offsets plus the complete encoded
                    // range are valid for every legacy file sharing the chunk.
                    entry.CompressedStartOffset = 0;
                    entry.CompressedEndOffset = checked((uint)encoded.Length);
                }
                writes.Add(new ChunkWrite(
                    chunkGroup.Key,
                    new FrostbiteCasLocation(first.OriginalInPatch, first.OriginalCatalog,
                        first.OriginalCas, first.OriginalOffset, first.OriginalCompressedSize),
                    encoded));
            }

            foreach (var entry in entries)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(
                    manifest.AsSpan(entry.RowPosition + 8, 4), entry.CompressedStartOffset);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    manifest.AsSpan(entry.RowPosition + 12, 4), entry.CompressedEndOffset);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    manifest.AsSpan(entry.RowPosition + 16, 4), entry.Offset);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    manifest.AsSpan(entry.RowPosition + 20, 4), entry.Size);
            }
            writes.Add(new ChunkWrite(
                manifestAsset.ChunkId,
                new FrostbiteCasLocation(manifestAsset.Patch, manifestAsset.Catalog,
                    manifestAsset.Cas, manifestAsset.Offset, manifestAsset.Size),
                Encode(root, manifest)));
        }

        return writes
            .GroupBy(x => x.Id)
            .Select(group => group.Count() == 1
                ? group.Single()
                : throw new InvalidDataException($"Chunk {group.Key} was modified more than once."))
            .ToList();
    }

    private static FrostbiteIndexedAsset CreateChunkAsset(LegacyAssetTarget target) =>
        new(FrostbiteAssetKind.Chunk, target.OriginalChunkId.ToString("D"),
            target.OriginalSha1, 0, 0, string.Empty, 0, target.OriginalChunkId,
            0, 0, target.OriginalInPatch, target.OriginalCatalog, target.OriginalCas,
            target.OriginalOffset, target.OriginalCompressedSize);

    private static ManifestEntry[] ParseManifest(byte[] manifest)
    {
        if (manifest.Length < ManifestHeaderSize)
            throw new InvalidDataException("ChunkFileCollector manifest is truncated.");
        var roots = BinaryPrimitives.ReadUInt32LittleEndian(manifest);
        var fileCount = BinaryPrimitives.ReadUInt32LittleEndian(manifest.AsSpan(12, 4));
        var fileOffset = BinaryPrimitives.ReadInt64LittleEndian(manifest.AsSpan(16, 8));
        var cacheCount = BinaryPrimitives.ReadUInt32LittleEndian(manifest.AsSpan(24, 4));
        var guidsOffset = BinaryPrimitives.ReadInt64LittleEndian(manifest.AsSpan(48, 8));
        if (fileCount > 2_000_000 || fileOffset < ManifestHeaderSize || guidsOffset < ManifestHeaderSize)
            throw new InvalidDataException("ChunkFileCollector has invalid offsets.");
        var relocationBytes = checked(((long)roots + cacheCount + 6) * 4);
        var guidBytes = manifest.Length - relocationBytes - guidsOffset;
        if (guidBytes < 0 || guidBytes % 16 != 0)
            throw new InvalidDataException("ChunkFileCollector GUID pool is invalid.");
        EnsureRange(manifest.Length, fileOffset, checked((long)fileCount * FileEntrySize));
        EnsureRange(manifest.Length, guidsOffset, guidBytes);
        var guidCount = checked((int)(guidBytes / 16));
        var result = new ManifestEntry[checked((int)fileCount)];
        for (var i = 0; i < result.Length; i++)
        {
            var rowPosition = checked((int)(fileOffset + (long)i * FileEntrySize));
            var row = manifest.AsSpan(rowPosition, FileEntrySize);
            var guidIndex = BinaryPrimitives.ReadInt32LittleEndian(row.Slice(24, 4));
            if ((uint)guidIndex >= (uint)guidCount)
                throw new InvalidDataException("ChunkFileCollector contains an invalid GUID reference.");
            var offset = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(16, 4));
            var size = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(20, 4));
            result[i] = new ManifestEntry
            {
                Hash = BinaryPrimitives.ReadUInt64LittleEndian(row),
                ChunkId = new Guid(manifest.AsSpan(checked((int)(guidsOffset + guidIndex * 16L)), 16)),
                RowPosition = rowPosition,
                OriginalOffset = offset,
                OriginalSize = size,
                CompressedStartOffset = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(8, 4)),
                CompressedEndOffset = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(12, 4)),
                Offset = offset,
                Size = size,
            };
        }
        return result;
    }

    private static Dictionary<Guid, TocChunkLocation> LocateDirectChunks(
        string root, HashSet<Guid> wanted)
    {
        var result = new Dictionary<Guid, TocChunkLocation>();
        foreach (var source in new[] { "Data", "Patch" })
        {
            var sourceRoot = Path.Combine(root, source);
            foreach (var toc in Directory.EnumerateFiles(sourceRoot, "*.toc", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(toc).Equals("layout.toc", StringComparison.OrdinalIgnoreCase)) continue;
                FrostbiteTocReadResult parsed;
                try { parsed = FrostbiteTocReader.Read(toc, Path.GetRelativePath(root, toc)); }
                catch (InvalidDataException) { continue; }
                foreach (var chunk in parsed.DirectChunks)
                    if (wanted.Contains(chunk.Id))
                        result[chunk.Id] = new TocChunkLocation(toc, chunk.LocationRecordPosition);
                if (result.Count == wanted.Count) return result;
            }
        }
        return result;
    }

    private static void Commit(
        string root,
        IReadOnlyDictionary<uint, string> catalogs,
        IReadOnlyList<ChunkWrite> writes,
        IReadOnlyDictionary<Guid, TocChunkLocation> tocLocations,
        string transaction)
    {
        var casLengths = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var tocStages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var plannedLocations = new Dictionary<Guid, FrostbiteCasLocation>();

        foreach (var group in writes.GroupBy(x => ResolveCasPath(root, catalogs, x.Original)))
        {
            var casPath = group.Key;
            var cursor = new FileInfo(casPath).Length;
            casLengths[casPath] = cursor;
            foreach (var write in group)
            {
                if (cursor > uint.MaxValue || write.Encoded.Length > uint.MaxValue - cursor)
                    throw new InvalidDataException("CAS file would exceed the FC26 32-bit offset limit.");
                plannedLocations[write.Id] = write.Original with
                {
                    Offset = checked((uint)cursor),
                    Size = checked((uint)write.Encoded.Length),
                };
                cursor += write.Encoded.Length;
            }
        }

        foreach (var tocPath in tocLocations.Values.Select(x => x.Path).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var stage = Path.Combine(transaction, "toc", Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(tocPath))) + ".toc");
            Directory.CreateDirectory(Path.GetDirectoryName(stage)!);
            File.Copy(tocPath, stage);
            tocStages[tocPath] = stage;
        }
        foreach (var write in writes)
        {
            var toc = tocLocations[write.Id];
            PatchTocLocation(tocStages[toc.Path], toc.RecordPosition, plannedLocations[write.Id]);
        }
        foreach (var stage in tocStages.Values)
        {
            SignToc(stage);
            _ = FrostbiteTocReader.Read(stage, Path.GetFileName(stage));
        }

        var replacedTocs = new List<(string Live, string Backup)>();
        try
        {
            foreach (var group in writes.GroupBy(x => ResolveCasPath(root, catalogs, x.Original)))
            {
                using var stream = new FileStream(group.Key, FileMode.Open, FileAccess.Write, FileShare.Read);
                stream.Position = stream.Length;
                foreach (var write in group)
                {
                    stream.Write(write.Encoded);
                    var decoded = FrostbitePayloadReader.Decompress(write.Encoded, 0, root);
                    if (decoded.Length == 0)
                        throw new InvalidDataException($"Verification failed for chunk {write.Id}.");
                }
                stream.Flush(flushToDisk: true);
            }

            foreach (var (live, stage) in tocStages)
            {
                var incoming = live + ".cm26-new-" + Guid.NewGuid().ToString("N");
                var backup = Path.Combine(transaction, "rollback-" + Guid.NewGuid().ToString("N") + ".toc");
                File.Copy(stage, incoming);
                File.Replace(incoming, live, backup, ignoreMetadataErrors: true);
                replacedTocs.Add((live, backup));
            }
        }
        catch
        {
            foreach (var (live, backup) in replacedTocs.AsEnumerable().Reverse())
                if (File.Exists(backup)) File.Copy(backup, live, overwrite: true);
            foreach (var (cas, length) in casLengths)
            {
                try
                {
                    using var stream = new FileStream(cas, FileMode.Open, FileAccess.Write, FileShare.Read);
                    stream.SetLength(length);
                    stream.Flush(flushToDisk: true);
                }
                catch { /* CmModData remains the final recovery source. */ }
            }
            throw;
        }
    }

    private static void VerifyTocPlan(
        string root,
        IReadOnlyDictionary<uint, string> catalogs,
        IReadOnlyList<ChunkWrite> writes,
        IReadOnlyDictionary<Guid, TocChunkLocation> tocLocations)
    {
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "cm26-toc-verify-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);
        try
        {
            var nextOffsets = writes
                .GroupBy(x => ResolveCasPath(root, catalogs, x.Original))
                .SelectMany(group =>
                {
                    var cursor = new FileInfo(group.Key).Length;
                    return group.Select(write =>
                    {
                        var location = write.Original with
                        {
                            Offset = checked((uint)cursor),
                            Size = checked((uint)write.Encoded.Length),
                        };
                        cursor = checked(cursor + write.Encoded.Length);
                        return (write.Id, location);
                    }).ToArray();
                }).ToDictionary(x => x.Id, x => x.location);
            var stages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tocPath in tocLocations.Values.Select(x => x.Path).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var stage = Path.Combine(temporaryRoot, Guid.NewGuid().ToString("N") + ".toc");
                File.Copy(tocPath, stage);
                stages[tocPath] = stage;
            }
            foreach (var write in writes)
            {
                var toc = tocLocations[write.Id];
                PatchTocLocation(stages[toc.Path], toc.RecordPosition, nextOffsets[write.Id]);
            }
            foreach (var stage in stages.Values)
            {
                SignToc(stage);
                _ = FrostbiteTocReader.Read(stage, Path.GetFileName(stage));
            }
        }
        finally
        {
            try { if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, recursive: true); }
            catch { }
        }
    }

    private static void PatchTocLocation(string path, long position, FrostbiteCasLocation location)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None);
        stream.Position = position;
        stream.WriteByte(0);
        stream.WriteByte(location.Patch ? (byte)1 : (byte)0);
        WriteUInt32BigEndian(stream, location.Catalog);
        stream.WriteByte(0);
        stream.WriteByte(location.Cas);
        WriteUInt32BigEndian(stream, location.Offset);
        WriteUInt32BigEndian(stream, location.Size);
        stream.Flush(flushToDisk: true);
    }

    private static void SignToc(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length <= TocHeaderSize) throw new InvalidDataException("TOC is truncated.");
        using var hmac = new HMACSHA1(TocHmacKey);
        var hash = hmac.ComputeHash(bytes, TocHeaderSize, bytes.Length - TocHeaderSize);
        using var key = CngKey.Import(Convert.FromBase64String(TocPrivateKey), CngKeyBlobFormat.GenericPrivateBlob);
        using var rsa = new RSACng(key);
        var signature = rsa.SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        if (signature.Length != 256) throw new InvalidDataException("FC26 TOC signature length is invalid.");
        signature.CopyTo(bytes, 8);
        File.WriteAllBytes(path, bytes);
    }

    private static byte[] Encode(string root, byte[] input)
    {
        using var output = new MemoryStream(input.Length);
        var libraryPath = Path.Combine(root, "oo2core_9_win64.dll");
        if (!File.Exists(libraryPath))
            throw new FileNotFoundException("FC26 Oodle library was not found.", libraryPath);
        nint library = 0;
        try
        {
            library = NativeLibrary.Load(libraryPath);
            var compressExport = NativeLibrary.GetExport(library, "OodleLZ_Compress");
            var boundExport = NativeLibrary.GetExport(library, "OodleLZ_GetCompressedBufferSizeNeeded");
            if (compressExport == 0 || boundExport == 0)
                throw new EntryPointNotFoundException("Oodle exports not found in oo2core_9_win64.dll");
            var compress = Marshal.GetDelegateForFunctionPointer<OodleCompress>(compressExport);
            var bound = Marshal.GetDelegateForFunctionPointer<OodleBound>(boundExport);
            for (var offset = 0; offset < input.Length; offset += FrostbiteBlockSize)
            {
                var count = Math.Min(FrostbiteBlockSize, input.Length - offset);
                var source = input.AsSpan(offset, count).ToArray();
                var destination = new byte[checked((int)bound(8, checked((nuint)count)))];
                var sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
                var destinationHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
                int packed;
                try
                {
                    packed = checked((int)compress(
                        8, sourceHandle.AddrOfPinnedObject(), checked((nuint)source.Length),
                        destinationHandle.AddrOfPinnedObject(), 4,
                        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0));
                }
                finally
                {
                    destinationHandle.Free();
                    sourceHandle.Free();
                }
                var useCompressed = packed > 0 && packed < count;
                WriteUInt32BigEndian(output, checked((uint)count));
                WriteUInt32BigEndian(output,
                    ((useCompressed ? 17u : 0u) << 24) | 0x00700000u |
                    checked((uint)(useCompressed ? packed : count)));
                output.Write(useCompressed ? destination.AsSpan(0, packed) : source);
            }
        }
        catch (AccessViolationException)
        {
            throw new InvalidOperationException(
                "Oodle compression failed — the oo2core_9_win64.dll may be corrupted or incompatible.");
        }
        catch (BadImageFormatException)
        {
            throw new InvalidOperationException(
                "The oo2core_9_win64.dll is not a valid 64-bit library.");
        }
        finally { if (library != 0) NativeLibrary.Free(library); }
        return output.ToArray();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nuint OodleCompress(
        int compressor, IntPtr raw, nuint rawLength, IntPtr compressed, int level,
        IntPtr options, IntPtr dictionaryBase, IntPtr lrm, IntPtr scratch, nuint scratchSize);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nuint OodleBound(int compressor, nuint rawSize);

    private static string ResolveCasPath(
        string root, IReadOnlyDictionary<uint, string> catalogs, FrostbiteCasLocation location)
    {
        if (!catalogs.TryGetValue(location.Catalog, out var catalog))
            throw new InvalidDataException($"Unknown FC26 catalog 0x{location.Catalog:X8}.");
        var source = location.Patch ? "Patch" : "Data";
        var sourceRoot = Path.GetFullPath(Path.Combine(root, source, "Win32"))
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(sourceRoot,
            catalog.Replace('/', Path.DirectorySeparatorChar), $"cas_{location.Cas:D2}.cas"));
        if (!path.StartsWith(sourceRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
            throw new InvalidDataException("Resolved CAS path is outside the FC26 archive root.");
        return path;
    }

    private static void EnsureSafeRoot(string root)
    {
        if (!Directory.Exists(Path.Combine(root, "Data")) ||
            !Directory.Exists(Path.Combine(root, "Patch")) ||
            !Directory.Exists(Path.Combine(root, "CmModData", "Data")) ||
            !Directory.Exists(Path.Combine(root, "CmModData", "Patch")))
            throw new InvalidOperationException(
                "Direct edit requires FC26 Data/Patch and a verified CmModData backup.");
    }

    /// <summary>
    /// A CmModData snapshot is safe only while it represents the exact installed
    /// game baseline.  A title update or a mod-manager folder swap changes the
    /// boot-critical layout/initfs files even when the database remains readable.
    /// Writing a database into that mixed state can leave FC26 unable to start,
    /// while the old snapshot is also unsafe to restore over the newer title
    /// update.  Check compact, immutable sentinels before any direct mutation.
    /// </summary>
    private static void EnsureBackupMatchesLiveGame(string root)
    {
        var mismatches = new List<string>();
        foreach (var relative in new[]
                 {
                     Path.Combine("Data", "layout.toc"),
                     Path.Combine("Data", "initfs_Win32"),
                     Path.Combine("Patch", "layout.toc"),
                     Path.Combine("Patch", "initfs_Win32"),
                 })
        {
            var live = Path.Combine(root, relative);
            var backup = Path.Combine(root, "CmModData", relative);
            if (!File.Exists(live) || !File.Exists(backup) ||
                new FileInfo(live).Length != new FileInfo(backup).Length ||
                !SHA256.HashData(File.ReadAllBytes(live)).AsSpan()
                    .SequenceEqual(SHA256.HashData(File.ReadAllBytes(backup))))
                mismatches.Add(relative.Replace(Path.DirectorySeparatorChar, '/'));
        }

        if (mismatches.Count != 0)
            throw new InvalidOperationException(
                "Direct save was blocked because the installed FC26 baseline no longer matches " +
                "CmModData (" + string.Join(", ", mismatches) + "). This usually means a title " +
                "update or an active FET/FIFAModData folder swap. Restore/repair FC26 with its " +
                "launcher, then create a fresh CmModData backup before editing. CM26 will not " +
                "write into a mixed game state.");
    }

    /// <summary>
    /// FC26 caches database-derived Assets files under Documents. FET clears this
    /// cache before launch whenever fifa_ng_db changes; do the same after a
    /// successful direct database transaction so stale asset indexes cannot be
    /// loaded against the new table layout.
    /// </summary>
    private static int ClearDatabaseAssetCaches()
    {
        try
        {
            var settings = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EA SPORTS FC 26", "settings");
            if (!Directory.Exists(settings)) return 0;
            var cleared = 0;
            foreach (var file in Directory.EnumerateFiles(settings, "Assets*", SearchOption.TopDirectoryOnly))
            {
                File.Delete(file);
                cleared++;
            }
            return cleared;
        }
        catch
        {
            // The game data has already committed atomically. Cache cleanup is
            // best-effort and must never convert a successful save into failure.
            return 0;
        }
    }

    private static void EnsureRange(long total, long offset, long length)
    {
        if (offset < 0 || length < 0 || offset > total || length > total - offset)
            throw new InvalidDataException("ChunkFileCollector range is outside the manifest.");
    }

    private static void WriteUInt32BigEndian(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        stream.Write(bytes);
    }
}
