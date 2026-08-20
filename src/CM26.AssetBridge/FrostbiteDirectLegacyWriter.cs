using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
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
    private sealed record FetChunkWrite(Guid Id, byte[] Encoded, ResolvedEdit Legacy);
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

    /// <summary>Writes the FETM v1 container consumed by FIFA Mod Manager for CM26 legacy edits.</summary>
    public static ApplyResult ExportFetMod(string gameRoot, string planPath, string destination)
    {
        var root = Path.GetFullPath(gameRoot);
        EnsureSafeRoot(root);
        var plan = JsonSerializer.Deserialize<Plan>(File.ReadAllText(planPath), BridgeJson.Options)
            ?? throw new InvalidDataException("FET export plan is empty.");
        var layout = FrostbiteLayoutReader.Read(Path.Combine(root, "Patch", "layout.toc"));
        var skipped = new List<string>();
        var edits = new List<ResolvedEdit>();
        foreach (var item in plan.Replacements ?? [])
        {
            // A missing staged payload is an export-integrity failure, not an
            // optional game asset.  Treating it as a skipped legacy path could
            // otherwise create a valid-looking but incomplete .fifamod.
            if (!File.Exists(item.SourcePath))
                throw new FileNotFoundException("Staged legacy replacement was not found.", item.SourcePath);
            try
            {
                var target = FrostbiteLegacyAssetResolver.ResolveTarget(root, layout.Catalogs, item.LegacyPath);
                edits.Add(new ResolvedEdit(target, File.ReadAllBytes(item.SourcePath)));
            }
            catch (FileNotFoundException) { skipped.Add(item.LegacyPath); }
        }
        if (edits.Count == 0) throw new InvalidDataException("No exportable CM26 legacy changes were staged.");
        // FIFA Mod Manager does not consume a rebuilt collector manifest from
        // the mod.  It creates one itself from legacy chunks.  Each file must
        // therefore become a new chunk with its actual logical size, just as
        // FET's LegacyFileManager does; reusing the original shared chunk with
        // a zero logical size produces a collector that crashes FC26 at boot.
        var writes = BuildFetChunkWrites(root, edits);
        WriteFetMod(destination, layout.Head, writes, edits);
        ValidateFetMod(destination);
        return new ApplyResult(edits.Count, skipped);
    }

    // This follows the FETM v1 reader layout used by FIFA Mod Manager.  The
    // exporter validates every generated package before reporting success so a
    // truncated manifest, wrong relative offset, or payload hash mismatch is
    // caught on the CM26 side instead of when a user attempts to import it.
    private static void ValidateFetMod(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        if (reader.ReadUInt32() != 1297368390u || reader.ReadByte() != 1)
            throw new InvalidDataException("Generated file does not contain a FETM v1 header.");
        if (!ReadString(reader).Equals("FC26", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Generated FETM package has an invalid game identifier.");
        _ = ReadUInt24(reader);
        for (var i = 0; i < 16; i++)
        {
            if (i == 2)
            {
                if (reader.ReadByte() != 6)
                    throw new InvalidDataException("Generated FETM package has an invalid main category.");
            }
            else if (i == 3)
            {
                if (reader.ReadByte() != 3)
                    throw new InvalidDataException("Generated FETM package has an invalid legacy subcategory.");
            }
            else _ = ReadString(reader);
        }
        Skip(reader, Read7Bit(reader)); // icon
        var screenshots = Read7Bit(reader);
        for (var i = 0; i < screenshots; i++) Skip(reader, Read7Bit(reader));
        var locales = Read7Bit(reader);
        for (var i = 0; i < locales; i++) { _ = ReadString(reader); _ = ReadString(reader); }
        var initFs = Read7Bit(reader);
        for (var i = 0; i < initFs; i++) { _ = ReadString(reader); Skip(reader, Read7Bit(reader)); }
        SkipLua(reader); SkipLua(reader);
        var manifestOffset = reader.ReadUInt32();
        if (manifestOffset >= stream.Length) throw new InvalidDataException("FETM manifest offset is outside the file.");
        var addedBundleCount = ReadUInt24(reader);
        for (var i = 0; i < addedBundleCount; i++) { _ = ReadString(reader); Skip(reader, 12); }
        SkipEbx(reader, ReadUInt24(reader));
        SkipRes(reader, ReadUInt24(reader));

        var chunks = new List<(byte[] Sha1, long Offset, int Length, ushort Flags, int LogicalSize)>();
        var chunkCount = ReadUInt24(reader);
        for (var i = 0; i < chunkCount; i++)
        {
            Skip(reader, 16);
            var sha1 = reader.ReadBytes(20);
            if (sha1.Length != 20) throw new EndOfStreamException();
            var flags = reader.ReadUInt16();
            var relativeOffset = Read7BitLong(reader);
            var length = Read7Bit(reader);
            if ((flags & 8) != 0) _ = Read7Bit(reader);
            var logicalSize = (flags & 16) != 0 ? Read7Bit(reader) : 0;
            if ((flags & 32) != 0) Skip(reader, 8); // FC26 H32
            if ((flags & 2) != 0) { Skip(reader, 8); _ = ReadString(reader); }
            if ((flags & 128) != 0) Skip(reader, checked(Read7Bit(reader) * 8L));
            if ((flags & 4) != 0) Skip(reader, 4);
            if (relativeOffset < 0 || length < 0 || relativeOffset > long.MaxValue - manifestOffset ||
                manifestOffset + relativeOffset > stream.Length - length)
                throw new InvalidDataException("FETM chunk payload range is invalid.");
            if ((flags & 2) == 0 || (flags & 16) == 0 || logicalSize == 0)
                throw new InvalidDataException("CM26 FET exports must contain sized legacy chunks only.");
            chunks.Add((sha1, relativeOffset, length, flags, logicalSize));
        }
        var collectors = Read7Bit(reader);
        if (chunks.Count == 0 || collectors == 0)
            throw new InvalidDataException("CM26 FET export is missing legacy chunks or collector metadata.");
        for (var i = 0; i < collectors; i++) { _ = ReadString(reader); Skip(reader, 21); }
        var bundleRefTables = Read7Bit(reader);
        for (var i = 0; i < bundleRefTables; i++) { Skip(reader, 4); _ = ReadString(reader); }
        foreach (var chunk in chunks)
        {
            stream.Position = manifestOffset + chunk.Offset;
            var payload = reader.ReadBytes(chunk.Length);
            if (payload.Length != chunk.Length || !SHA1.HashData(payload).AsSpan().SequenceEqual(chunk.Sha1))
                throw new InvalidDataException("FETM chunk payload SHA1 verification failed.");
        }
    }

    private static void SkipLua(BinaryReader reader)
    {
        var count = Read7Bit(reader);
        for (var i = 0; i < count; i++)
        {
            _ = ReadString(reader);
            var lines = Read7Bit(reader);
            for (var line = 0; line < lines; line++) _ = ReadString(reader);
        }
    }

    private static void SkipEbx(BinaryReader reader, uint count)
    {
        if (count != 0) throw new InvalidDataException("CM26 FET validator only accepts packages without EBX entries.");
    }

    private static void SkipRes(BinaryReader reader, uint count)
    {
        if (count != 0) throw new InvalidDataException("CM26 FET validator only accepts packages without RES entries.");
    }

    private static string ReadString(BinaryReader reader)
    {
        var length = Read7Bit(reader);
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length) throw new EndOfStreamException();
        return Encoding.UTF8.GetString(bytes);
    }
    private static int Read7Bit(BinaryReader reader)
    {
        var value = 0; var shift = 0;
        while (true)
        {
            var next = reader.ReadByte();
            value |= (next & 0x7F) << shift;
            if ((next & 0x80) == 0) return value;
            shift += 7;
            if (shift > 28) throw new InvalidDataException($"Invalid FETM 7-bit integer at offset {reader.BaseStream.Position}.");
        }
    }
    private static long Read7BitLong(BinaryReader reader)
    {
        long value = 0; var shift = 0;
        while (true)
        {
            var next = reader.ReadByte();
            value |= (long)(next & 0x7F) << shift;
            if ((next & 0x80) == 0) return value;
            shift += 7;
            if (shift > 63) throw new InvalidDataException("Invalid FETM 7-bit long.");
        }
    }
    private static uint ReadUInt24(BinaryReader reader)
    {
        var b0 = reader.ReadByte(); var b1 = reader.ReadByte(); var b2 = reader.ReadByte();
        return (uint)(b0 | (b1 << 8) | (b2 << 16));
    }
    private static void Skip(BinaryReader reader, long bytes)
    {
        if (bytes < 0 || bytes > reader.BaseStream.Length - reader.BaseStream.Position)
            throw new EndOfStreamException("FETM structure is truncated.");
        reader.BaseStream.Position += bytes;
    }

    private static void WriteFetMod(string destination, int head, IReadOnlyList<FetChunkWrite> writes,
        IReadOnlyList<ResolvedEdit> edits)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
        using var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(1297368390u); // FETM
        writer.Write((byte)1);
        WriteString(writer, "FC26");
        WriteUInt24(writer, checked((uint)head));
        WriteString(writer, Path.GetFileNameWithoutExtension(destination));
        WriteString(writer, "CM26");
        // FIFA Mod Manager maps this pair through ModMainCategory.  Zero is
        // not a defined category and causes its importer to fail with
        // "Parameter 'element'".  CM26's current payloads are legacy
        // database/lightweight asset edits, so use Legacy / Database.
        writer.Write((byte)6); writer.Write((byte)3);
        foreach (var value in new[] { "", "", "1.0", "Created by Creation Master 26", "", "", "", "", "", "", "", "" })
            WriteString(writer, value);
        Write7Bit(writer, 0); // icon
        Write7Bit(writer, 0); // screenshots
        Write7Bit(writer, 0); // locale ini
        Write7Bit(writer, 0); // initfs
        Write7Bit(writer, 0); Write7Bit(writer, 0); // lua changes
        var manifestOffsetPosition = stream.Position;
        writer.Write(0u);
        WriteUInt24(writer, 0); // added bundles
        WriteUInt24(writer, 0); // EBX
        WriteUInt24(writer, 0); // RES
        WriteUInt24(writer, checked((uint)writes.Count));
        var payloads = new List<byte[]>();
        long payloadOffset = 0;
        foreach (var write in writes)
        {
            writer.Write(write.Id.ToByteArray().AsSpan());
            var payload = write.Encoded;
            writer.Write(System.Security.Cryptography.SHA1.HashData(payload).AsSpan());
            writer.Write((ushort)(2 | 16)); // IsLegacy + HasLogicalSize
            Write7BitLong(writer, payloadOffset);
            Write7Bit(writer, payload.Length);
            Write7Bit(writer, write.Legacy.ReplacementBytes.Length);
            writer.Write(write.Legacy.Target.NameHash);
            WriteString(writer, write.Legacy.Target.Name);
            payloadOffset += payload.Length;
            payloads.Add(payload);
        }
        var collectors = edits.GroupBy(edit => edit.Target.CollectorEbxName, StringComparer.OrdinalIgnoreCase).ToArray();
        Write7Bit(writer, collectors.Length);
        foreach (var group in collectors)
        {
            var target = group.First().Target;
            WriteString(writer, target.CollectorEbxName);
            writer.Write(target.CollectorManifestChunkId.ToByteArray().AsSpan());
            writer.Write(target.CollectorInPatch);
            writer.Write(SuperBundleHash(target.CollectorSuperBundle));
        }
        Write7Bit(writer, 0); // bundle ref tables
        var manifestStart = stream.Position;
        foreach (var payload in payloads) writer.Write(payload);
        stream.Position = manifestOffsetPosition;
        writer.Write(checked((uint)manifestStart));
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value ?? string.Empty);
        Write7Bit(writer, bytes.Length); writer.Write(bytes);
    }
    private static void Write7Bit(BinaryWriter writer, int value)
    {
        uint current = checked((uint)value);
        while (current >= 128) { writer.Write((byte)(current | 128)); current >>= 7; }
        writer.Write((byte)current);
    }
    private static void Write7BitLong(BinaryWriter writer, long value)
    {
        ulong current = checked((ulong)value);
        while (current >= 128) { writer.Write((byte)(current | 128)); current >>= 7; }
        writer.Write((byte)current);
    }
    private static void WriteUInt24(BinaryWriter writer, uint value)
    {
        writer.Write((byte)value); writer.Write((byte)(value >> 8)); writer.Write((byte)(value >> 16));
    }
    private static uint SuperBundleHash(string value) => XxHash32(System.Text.Encoding.Unicode.GetBytes(value.ToLowerInvariant()));

    private static IReadOnlyList<FetChunkWrite> BuildFetChunkWrites(string root, IReadOnlyList<ResolvedEdit> edits)
    {
        var used = new HashSet<Guid>();
        var writes = new List<FetChunkWrite>(edits.Count);
        Span<byte> guidBytes = stackalloc byte[16];
        foreach (var edit in edits)
        {
            // Match FET's FC26 LegacyFileManager.GenerateDeterministicGuid:
            // [nameHash, nameHash ^ counter], then force the marker byte.
            // FIFA Mod Manager uses this shape for one-file legacy chunks.
            var counter = 1UL;
            Guid id;
            do
            {
                BinaryPrimitives.WriteUInt64LittleEndian(guidBytes, edit.Target.NameHash);
                BinaryPrimitives.WriteUInt64LittleEndian(guidBytes[8..], edit.Target.NameHash ^ counter++);
                guidBytes[15] = 1;
                id = new Guid(guidBytes);
            }
            while (id == edit.Target.OriginalChunkId || !used.Add(id));
            var encoded = Encode(root, edit.ReplacementBytes);
            var decoded = FrostbitePayloadReader.Decompress(encoded, edit.ReplacementBytes.Length, root);
            if (!decoded.AsSpan().SequenceEqual(edit.ReplacementBytes))
                throw new InvalidDataException($"FET legacy payload verification failed for {edit.Target.Name}.");
            writes.Add(new FetChunkWrite(id, encoded, edit));
        }
        return writes;
    }
    private static uint XxHash32(ReadOnlySpan<byte> data)
    {
        const uint p1 = 2654435761, p2 = 2246822519, p3 = 3266489917, p4 = 668265263, p5 = 374761393;
        uint h = p5 + (uint)data.Length; var i = 0;
        while (i + 4 <= data.Length) { h += BitConverter.ToUInt32(data.Slice(i, 4)) * p3; h = BitOperations.RotateLeft(h, 17) * p4; i += 4; }
        while (i < data.Length) { h += data[i++] * p5; h = BitOperations.RotateLeft(h, 11) * p1; }
        h ^= h >> 15; h *= p2; h ^= h >> 13; h *= p3; return h ^ (h >> 16);
    }

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
            catch (Exception cleanupEx)
            {
                // A stale transaction folder is safe and can be cleaned later,
                // but record it so it is not invisible in diagnostics.
                System.Diagnostics.Debug.WriteLine($"[CM26] Transaction cleanup failed: {cleanupEx.Message}");
            }
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
            0, 0, string.Empty, target.OriginalInPatch, target.OriginalCatalog, target.OriginalCas,
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

        // CM26ModData starts as a file-symlink mirror of the installed game.
        // Break only links that this transaction will mutate. Without this
        // copy-on-write step, appending to a CAS link would modify the original
        // FC26 archive through the reparse point.
        foreach (var path in writes.Select(x => ResolveCasPath(root, catalogs, x.Original))
                     .Concat(tocLocations.Values.Select(x => x.Path))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
            MaterializeSymbolicLink(path);

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
                catch (Exception rollbackEx)
                {
                    // CmModData remains the final recovery source, but never lose
                    // the fact that a rollback step itself failed.
                    System.Diagnostics.Debug.WriteLine($"[CM26] CAS rollback failed for {cas}: {rollbackEx.Message}");
                }
            }
            throw;
        }
    }

    private static void MaterializeSymbolicLink(string path)
    {
        var file = new FileInfo(path);
        if (file.LinkTarget is null) return;
        var target = file.ResolveLinkTarget(returnFinalTarget: true)
            ?? throw new IOException("CM26 could not resolve overlay link: " + path);
        var materialized = path + ".cm26-cow-" + Guid.NewGuid().ToString("N");
        try
        {
            File.Copy(target.FullName, materialized, overwrite: false);
            File.Delete(path);
            File.Move(materialized, path);
        }
        finally
        {
            if (File.Exists(materialized)) File.Delete(materialized);
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
            catch (Exception ex) { Debug.WriteLine("CM26 TOC verification cleanup failed: " + ex.Message); }
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
                // FC26/FET use the Leviathan Frostbite codec for legacy chunks.
                // The previous Kraken marker was readable by the bridge but is
                // not the codec FMM emits for FC26 legacy files.
                var destination = new byte[checked((int)bound(13, checked((nuint)count)))];
                var sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
                var destinationHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
                int packed;
                try
                {
                    packed = checked((int)compress(
                        13, sourceHandle.AddrOfPinnedObject(), checked((nuint)source.Length),
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
                    ((useCompressed ? 24u : 0u) << 24) | 0x00700000u |
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
