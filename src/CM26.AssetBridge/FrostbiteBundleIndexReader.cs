using System.Buffers.Binary;
using System.Text;

namespace CM26.AssetBridge;

internal sealed record FrostbiteBundleIndex(
    int EbxCount,
    int ResCount,
    int ChunkCount,
    IReadOnlyList<string> EbxNames,
    IReadOnlyList<string> ResNames,
    IReadOnlyList<FrostbiteBundleAsset> Assets);

internal enum FrostbiteAssetKind : byte
{
    Ebx = 1,
    Res = 2,
    Chunk = 3,
}

internal sealed record FrostbiteBundleAsset(
    FrostbiteAssetKind Kind,
    string Name,
    string Sha1,
    uint OriginalSize,
    uint ResType = 0,
    string ResMeta = "",
    ulong ResRid = 0,
    Guid ChunkId = default,
    uint LogicalOffset = 0,
    uint LogicalSize = 0);

/// <summary>
/// Reads only the asset manifest stored at the head of a CAS bundle. Payload
/// blocks remain untouched and are not decompressed during automatic indexing.
/// </summary>
internal static class FrostbiteBundleIndexReader
{
    private const uint BundleMagic = 0xD68E799D;
    private const int MaxAssetCount = 2_000_000;
    private const int MaxBundleMetadataBytes = 128 * 1024 * 1024;
    private const int MaxNameBytes = 16 * 1024;

    public static FrostbiteBundleIndex Read(
        string path, uint bundleOffset, uint bundleSize)
    {
        if (bundleSize <= 4 || bundleSize > MaxBundleMetadataBytes)
            throw new InvalidDataException($"Invalid bundle metadata size: {bundleSize}.");

        var payloadOffset = checked((long)bundleOffset + 4);
        var payloadLength = checked((int)bundleSize - 4);
        using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            bufferSize: 64 * 1024, FileOptions.RandomAccess);
        if (payloadOffset > stream.Length || payloadLength > stream.Length - payloadOffset)
            throw new InvalidDataException("Bundle metadata lies outside its CAS file.");

        var data = new byte[payloadLength];
        stream.Position = payloadOffset;
        stream.ReadExactly(data);
        return Parse(data);
    }

    private static FrostbiteBundleIndex Parse(ReadOnlySpan<byte> data)
    {
        var cursor = 0;
        var magic = ReadUInt32BigEndian(data, ref cursor);
        if (magic != BundleMagic)
            throw new InvalidDataException($"Invalid CAS bundle magic 0x{magic:X8}.");

        var totalCount = ReadUInt32LittleEndian(data, ref cursor);
        var ebxCount = ReadUInt32LittleEndian(data, ref cursor);
        var resCount = ReadUInt32LittleEndian(data, ref cursor);
        var chunkCount = ReadUInt32LittleEndian(data, ref cursor);
        var stringOffset = ReadUInt32LittleEndian(data, ref cursor);
        _ = ReadUInt32LittleEndian(data, ref cursor); // chunk metadata offset
        _ = ReadUInt32LittleEndian(data, ref cursor); // chunk metadata size

        ValidateCount(totalCount, "total");
        ValidateCount(ebxCount, "EBX");
        ValidateCount(resCount, "RES");
        ValidateCount(chunkCount, "chunk");
        if (totalCount != checked(ebxCount + resCount + chunkCount))
            throw new InvalidDataException("Bundle asset counts are inconsistent.");
        EnsureRange(data, cursor, checked((int)totalCount * 20), "SHA1 table");
        var hashes = new string[checked((int)totalCount)];
        for (var i = 0; i < hashes.Length; i++)
        {
            hashes[i] = Convert.ToHexString(data.Slice(cursor, 20));
            cursor += 20;
        }

        var assets = new FrostbiteBundleAsset[checked((int)totalCount)];
        var ebxNames = ReadNamedEntries(
            data, ref cursor, stringOffset, ebxCount, hashes, 0,
            FrostbiteAssetKind.Ebx, assets);
        var resStart = checked((int)ebxCount);
        var resNames = ReadNamedEntries(
            data, ref cursor, stringOffset, resCount, hashes, resStart,
            FrostbiteAssetKind.Res, assets);

        EnsureRange(data, cursor, checked((int)resCount * 4), "RES type table");
        for (var i = 0; i < resCount; i++)
        {
            var type = ReadUInt32LittleEndian(data, ref cursor);
            assets[resStart + i] = assets[resStart + i] with { ResType = type };
        }
        EnsureRange(data, cursor, checked((int)resCount * 16), "RES metadata table");
        for (var i = 0; i < resCount; i++)
        {
            var meta = Convert.ToHexString(data.Slice(cursor, 16));
            cursor += 16;
            assets[resStart + i] = assets[resStart + i] with { ResMeta = meta };
        }
        EnsureRange(data, cursor, checked((int)resCount * 8), "RES RID table");
        for (var i = 0; i < resCount; i++)
        {
            var rid = ReadUInt64LittleEndian(data, ref cursor);
            assets[resStart + i] = assets[resStart + i] with { ResRid = rid };
        }
        EnsureRange(data, cursor, checked((int)chunkCount * 24), "chunk manifest");
        var chunkStart = checked(resStart + (int)resCount);
        for (var i = 0; i < chunkCount; i++)
        {
            var chunkId = new Guid(data.Slice(cursor, 16));
            cursor += 16;
            var logicalOffset = ReadUInt32LittleEndian(data, ref cursor);
            var logicalSize = ReadUInt32LittleEndian(data, ref cursor);
            var hashIndex = chunkStart + i;
            assets[hashIndex] = new FrostbiteBundleAsset(
                FrostbiteAssetKind.Chunk, chunkId.ToString("D"), hashes[hashIndex], 0,
                ChunkId: chunkId, LogicalOffset: logicalOffset, LogicalSize: logicalSize);
        }

        return new FrostbiteBundleIndex(
            checked((int)ebxCount), checked((int)resCount), checked((int)chunkCount),
            ebxNames, resNames, assets);
    }

    private static IReadOnlyList<string> ReadNamedEntries(
        ReadOnlySpan<byte> data,
        ref int cursor,
        uint stringOffset,
        uint count,
        IReadOnlyList<string> hashes,
        int hashStart,
        FrostbiteAssetKind kind,
        FrostbiteBundleAsset[] assets)
    {
        var result = new string[checked((int)count)];
        for (var i = 0; i < result.Length; i++)
        {
            var nameOffset = ReadUInt32LittleEndian(data, ref cursor);
            var originalSize = ReadUInt32LittleEndian(data, ref cursor);
            var namePosition = checked((int)(stringOffset + nameOffset));
            result[i] = ReadNullTerminatedUtf8(data, namePosition);
            var hashIndex = hashStart + i;
            assets[hashIndex] = new FrostbiteBundleAsset(
                kind, result[i], hashes[hashIndex], originalSize);
        }
        return result;
    }

    private static string ReadNullTerminatedUtf8(ReadOnlySpan<byte> data, int offset)
    {
        EnsureRange(data, offset, 1, "asset name");
        var remaining = data[offset..];
        var terminator = remaining.IndexOf((byte)0);
        if (terminator < 0 || terminator > MaxNameBytes)
            throw new InvalidDataException("Asset name is unterminated or exceeds the safety limit.");
        return Encoding.UTF8.GetString(remaining[..terminator]);
    }

    private static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> data, ref int cursor)
    {
        EnsureRange(data, cursor, 4, "bundle field");
        var result = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(cursor, 4));
        cursor += 4;
        return result;
    }

    private static uint ReadUInt32BigEndian(ReadOnlySpan<byte> data, ref int cursor)
    {
        EnsureRange(data, cursor, 4, "bundle field");
        var result = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(cursor, 4));
        cursor += 4;
        return result;
    }

    private static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> data, ref int cursor)
    {
        EnsureRange(data, cursor, 8, "bundle field");
        var result = BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(cursor, 8));
        cursor += 8;
        return result;
    }

    private static void ValidateCount(uint count, string label)
    {
        if (count > MaxAssetCount)
            throw new InvalidDataException($"Invalid {label} asset count: {count}.");
    }

    private static void EnsureRange(ReadOnlySpan<byte> data, int offset, int length, string label)
    {
        if (offset < 0 || length < 0 || offset > data.Length || length > data.Length - offset)
            throw new InvalidDataException($"{label} lies outside the bundle metadata.");
    }
}
