using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace CM26.AssetBridge;

internal sealed record FrostbiteCasLocation(
    bool Patch, uint Catalog, byte Cas, uint Offset, uint Size);

internal sealed record FrostbiteCasBundle(
    string Name,
    string SuperBundle,
    FrostbiteCasLocation Metadata,
    IReadOnlyList<FrostbiteCasLocation> Assets);

internal sealed record FrostbiteTocReadResult(
    FrostbiteTocIndex Index,
    IReadOnlyList<FrostbiteCasBundle> CasBundles,
    IReadOnlyList<FrostbiteDirectChunk> DirectChunks);

/// <summary>
/// A chunk whose CAS location is declared directly by a superbundle TOC.
/// These are not part of a CAS bundle manifest, and include the payloads used
/// by FC26's legacy-file collector.
/// </summary>
internal sealed record FrostbiteDirectChunk(
    Guid Id, string SuperBundle, FrostbiteCasLocation Location, long LocationRecordPosition);

/// <summary>
/// Bounded, read-only reader for the FC26 superbundle TOC index. This reader
/// only interprets the index/header layer; CAS payloads are handled separately.
/// </summary>
internal static class FrostbiteTocReader
{
    private const int TocHeaderSize = 556;
    private const uint CompressedStringsFlag = 4;
    private const int MaxEntryCount = 2_000_000;
    private const int MaxBundleNameLength = 16_384;

    public static FrostbiteTocReadResult Read(string absolutePath, string relativePath)
    {
        using var stream = new FileStream(
            absolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            bufferSize: 64 * 1024, FileOptions.SequentialScan);
        if (stream.Length < TocHeaderSize + 48)
            throw new InvalidDataException("File is too short for a superbundle TOC.");

        stream.Position = TocHeaderSize;
        _ = ReadUInt32BigEndian(stream);
        var bundleDataOffset = ReadUInt32BigEndian(stream);
        var bundleCount = ReadInt32BigEndian(stream);
        _ = ReadUInt32BigEndian(stream);
        var chunkGuidOffset = ReadUInt32BigEndian(stream);
        var chunkCount = ReadInt32BigEndian(stream);
        _ = ReadUInt32BigEndian(stream);
        _ = ReadUInt32BigEndian(stream);
        var namesOffset = ReadUInt32BigEndian(stream);
        var dataOffset = ReadUInt32BigEndian(stream);
        _ = ReadUInt32BigEndian(stream);
        var flags = ReadUInt32BigEndian(stream);

        ValidateCount(bundleCount, "bundle");
        ValidateCount(chunkCount, "chunk");

        uint namesWordCount = 0;
        uint decodeTableCount = 0;
        uint decodeTableOffset = 0;
        if ((flags & CompressedStringsFlag) != 0)
        {
            namesWordCount = ReadUInt32BigEndian(stream);
            decodeTableCount = ReadUInt32BigEndian(stream);
            decodeTableOffset = ReadUInt32BigEndian(stream);
            ValidateArrayCount(namesWordCount, sizeof(uint), "compressed names");
            ValidateArrayCount(decodeTableCount, sizeof(int), "name decode table");
        }

        var bundleTablePosition = AddOffset(bundleDataOffset);
        EnsureRange(stream, bundleTablePosition, checked((long)bundleCount * 16), "bundle table");
        stream.Position = bundleTablePosition;
        var rawBundles = new RawBundle[bundleCount];
        for (var i = 0; i < bundleCount; i++)
            rawBundles[i] = new RawBundle(
                ReadUInt32BigEndian(stream),
                ReadUInt32BigEndian(stream),
                ReadUInt64BigEndian(stream));

        var superBundle = ToSuperBundleName(relativePath);
        var directChunks = ReadDirectChunks(
            stream, chunkGuidOffset, dataOffset, chunkCount, superBundle);

        IReadOnlyList<string> names = (flags & CompressedStringsFlag) != 0
            ? ReadCompressedNames(stream, rawBundles, namesOffset, namesWordCount,
                decodeTableOffset, decodeTableCount)
            : ReadPlainNames(stream, rawBundles, namesOffset);

        var casBundles = ReadCasBundles(stream, rawBundles, names, superBundle);
        return new FrostbiteTocReadResult(
            new FrostbiteTocIndex(relativePath, flags, bundleCount, chunkCount, names),
            casBundles, directChunks);
    }

    private static IReadOnlyList<FrostbiteDirectChunk> ReadDirectChunks(
        Stream stream, uint chunkGuidOffset, uint dataOffset, int chunkCount, string superBundle)
    {
        if (chunkCount == 0) return [];

        // FC21+ TOCs keep GUID + 32-bit location-table index together. The
        // location table itself contains one 16-byte CAS location per chunk.
        var guidTablePosition = AddOffset(chunkGuidOffset);
        EnsureRange(stream, guidTablePosition, checked((long)chunkCount * 20), "chunk GUID table");
        var chunkDataPosition = AddOffset(dataOffset);
        EnsureRange(stream, chunkDataPosition, checked((long)chunkCount * 16), "chunk location table");

        var descriptors = new Dictionary<uint, Guid>(chunkCount);
        stream.Position = guidTablePosition;
        Span<byte> guidBytes = stackalloc byte[16];
        for (var i = 0; i < chunkCount; i++)
        {
            stream.ReadExactly(guidBytes);
            // FC26 TOCs store chunk GUIDs as a fully reversed byte sequence.
            // This matches FET's ReadReversedGuidLittleEndian implementation.
            guidBytes.Reverse();
            var id = new Guid(guidBytes);
            var decodeAndOffset = ReadUInt32BigEndian(stream);
            var locationIndex = decodeAndOffset & 0x00FFFFFF;
            descriptors[locationIndex] = id;
        }

        stream.Position = chunkDataPosition;
        var result = new List<FrostbiteDirectChunk>(chunkCount);
        for (uint index = 0; index < chunkCount; index++)
        {
            // The TOC stores the descriptor offset in 32-bit words, while a
            // FC26 chunk-location record occupies four words (16 bytes).
            if (!descriptors.TryGetValue(checked(index * 4), out var id))
                throw new InvalidDataException("Chunk location table has no matching GUID.");
            EnsureRange(stream, stream.Position, 16, "chunk CAS location");
            _ = stream.ReadByte();
            var patch = stream.ReadByte() != 0;
            var catalog = ReadUInt32BigEndian(stream);
            _ = stream.ReadByte();
            var cas = checked((byte)stream.ReadByte());
            var offset = ReadUInt32BigEndian(stream);
            var size = ReadUInt32BigEndian(stream);
            result.Add(new FrostbiteDirectChunk(
                id, superBundle, new FrostbiteCasLocation(patch, catalog, cas, offset, size),
                checked(chunkDataPosition + (long)index * 16)));
        }
        return result;
    }

    private static IReadOnlyList<FrostbiteCasBundle> ReadCasBundles(
        Stream stream, IReadOnlyList<RawBundle> bundles, IReadOnlyList<string> names, string superBundle)
    {
        const uint memoryResident = 0x80000000;
        const uint inlineRead = 0x40000000;
        var result = new List<FrostbiteCasBundle>();
        for (var bundleIndex = 0; bundleIndex < bundles.Count; bundleIndex++)
        {
            var bundle = bundles[bundleIndex];
            if ((bundle.EncodedLength & (memoryResident | inlineRead)) == 0) continue;

            var start = checked(TocHeaderSize + (long)bundle.Offset);
            EnsureRange(stream, start, 36, "CAS bundle location header");
            stream.Position = start;
            _ = ReadInt32BigEndian(stream);
            _ = ReadInt32BigEndian(stream);
            var flagsOffset = ReadInt32BigEndian(stream);
            var entryCount = ReadInt32BigEndian(stream);
            var entriesOffset = ReadInt32BigEndian(stream);
            _ = ReadInt32BigEndian(stream);
            _ = ReadInt32BigEndian(stream);
            _ = ReadInt32BigEndian(stream);
            _ = ReadInt32BigEndian(stream); // FC25+ extended header field
            ValidateCount(entryCount, "CAS bundle entry");
            if (entryCount == 0) continue;

            var flagsPosition = checked(start + flagsOffset);
            var entriesPosition = checked(start + entriesOffset);
            EnsureRange(stream, flagsPosition, entryCount, "CAS bundle flags");
            var entryFlags = new byte[entryCount];
            stream.Position = flagsPosition;
            stream.ReadExactly(entryFlags);

            stream.Position = entriesPosition;
            var locations = new List<FrostbiteCasLocation>(entryCount);
            var patch = false;
            uint catalog = uint.MaxValue;
            byte cas = 0;
            for (var i = 0; i < entryCount; i++)
            {
                if (entryFlags[i] == 128)
                {
                    EnsureRange(stream, stream.Position, 8, "CAS identifier");
                    _ = stream.ReadByte();
                    patch = stream.ReadByte() != 0;
                    catalog = ReadUInt32BigEndian(stream);
                    _ = stream.ReadByte();
                    cas = checked((byte)stream.ReadByte());
                }
                EnsureRange(stream, stream.Position, 8, "CAS location");
                locations.Add(new FrostbiteCasLocation(
                    patch, catalog, cas,
                    ReadUInt32BigEndian(stream), ReadUInt32BigEndian(stream)));
            }

            result.Add(new FrostbiteCasBundle(
                names[bundleIndex], superBundle, locations[0], locations.Skip(1).ToArray()));
        }
        return result;
    }

    private static string ToSuperBundleName(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/');
        foreach (var prefix in new[] { "Data/", "Patch/" })
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                normalized = normalized[prefix.Length..];
        if (normalized.StartsWith("Win32/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized["Win32/".Length..];
        return Path.ChangeExtension(normalized, null)?.Replace('\\', '/').ToLowerInvariant()
            ?? throw new InvalidDataException("TOC has no superbundle name.");
    }

    private static IReadOnlyList<string> ReadCompressedNames(
        Stream stream,
        IReadOnlyList<RawBundle> bundles,
        uint namesOffset,
        uint namesWordCount,
        uint decodeTableOffset,
        uint decodeTableCount)
    {
        if (namesWordCount == 0 || decodeTableCount < 2)
        {
            if (bundles.Count == 0) return [];
            throw new InvalidDataException("Compressed name data is empty.");
        }

        var words = new uint[checked((int)namesWordCount)];
        var wordsPosition = AddOffset(namesOffset);
        EnsureRange(stream, wordsPosition, checked((long)words.Length * 4), "compressed names");
        stream.Position = wordsPosition;
        for (var i = 0; i < words.Length; i++) words[i] = ReadUInt32BigEndian(stream);

        var table = new int[checked((int)decodeTableCount)];
        var tablePosition = AddOffset(decodeTableOffset);
        EnsureRange(stream, tablePosition, checked((long)table.Length * 4), "name decode table");
        stream.Position = tablePosition;
        for (var i = 0; i < table.Length; i++) table[i] = ReadInt32BigEndian(stream);

        var result = new string[bundles.Count];
        for (var i = 0; i < bundles.Count; i++)
            result[i] = DecodeName(words, table, bundles[i].NameBitOffset);
        return result;
    }

    private static IReadOnlyList<string> ReadPlainNames(
        Stream stream, IReadOnlyList<RawBundle> bundles, uint namesOffset)
    {
        var result = new string[bundles.Count];
        for (var i = 0; i < bundles.Count; i++)
        {
            var position = checked(AddOffset(namesOffset) + bundles[i].NameBitOffset);
            EnsureRange(stream, position, 1, "bundle name");
            stream.Position = position;
            result[i] = ReadNullTerminatedUtf8(stream);
        }
        return result;
    }

    private static string DecodeName(uint[] words, int[] table, uint startBit)
    {
        var bitIndex = (long)startBit;
        var maxBits = checked((long)words.Length * 32);
        var builder = new StringBuilder();
        while (builder.Length < MaxBundleNameLength)
        {
            var node = table.Length / 2 - 1;
            while (node >= 0)
            {
                if (bitIndex >= maxBits)
                    throw new InvalidDataException("Compressed bundle name exceeds its bit stream.");
                var tableIndex = checked(node * 2 + (int)((words[bitIndex / 32] >> (int)(bitIndex % 32)) & 1));
                if ((uint)tableIndex >= (uint)table.Length)
                    throw new InvalidDataException("Compressed bundle name references an invalid decode node.");
                node = table[tableIndex];
                bitIndex++;
            }

            var value = -1 - node;
            if (value == 0) return builder.ToString();
            if ((uint)value > char.MaxValue)
                throw new InvalidDataException("Compressed bundle name contains an invalid character.");
            builder.Append((char)value);
        }
        throw new InvalidDataException("Bundle name exceeds the safety limit.");
    }

    private static string ReadNullTerminatedUtf8(Stream stream)
    {
        using var bytes = new MemoryStream();
        for (var i = 0; i < MaxBundleNameLength; i++)
        {
            var value = stream.ReadByte();
            if (value < 0) throw new EndOfStreamException("Unterminated bundle name.");
            if (value == 0) return Encoding.UTF8.GetString(bytes.GetBuffer(), 0, checked((int)bytes.Length));
            bytes.WriteByte((byte)value);
        }
        throw new InvalidDataException("Bundle name exceeds the safety limit.");
    }

    private static long AddOffset(uint relativeOffset) => checked(TocHeaderSize + (long)relativeOffset);

    private static void ValidateCount(int count, string label)
    {
        if (count < 0 || count > MaxEntryCount)
            throw new InvalidDataException($"Invalid {label} count: {count}.");
    }

    private static void ValidateArrayCount(uint count, int elementSize, string label)
    {
        if (count > MaxEntryCount)
            throw new InvalidDataException($"Invalid {label} count: {count}.");
        _ = checked((long)count * elementSize);
    }

    private static void EnsureRange(Stream stream, long offset, long length, string label)
    {
        if (offset < 0 || length < 0 || offset > stream.Length || length > stream.Length - offset)
            throw new InvalidDataException($"{label} lies outside the TOC file.");
    }

    // These helpers deliberately stay out-of-line: the TOC has very large
    // tables, and inlining stackalloc-based helpers into a tight loop can make
    // the JIT reserve stack space once per iteration.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static uint ReadUInt32BigEndian(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[4];
        stream.ReadExactly(bytes);
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ReadInt32BigEndian(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[4];
        stream.ReadExactly(bytes);
        return BinaryPrimitives.ReadInt32BigEndian(bytes);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong ReadUInt64BigEndian(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[8];
        stream.ReadExactly(bytes);
        return BinaryPrimitives.ReadUInt64BigEndian(bytes);
    }

    private readonly record struct RawBundle(uint NameBitOffset, uint EncodedLength, ulong Offset);
}
