using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;
using ZstdSharp;

namespace CM26.AssetBridge;

internal static class FrostbitePayloadReader
{
    private const int MaxOutputBytes = 512 * 1024 * 1024;

    public static string Extract(
        string gameRoot,
        IReadOnlyDictionary<uint, string> catalogs,
        FrostbiteIndexedAsset asset)
    {
        var payload = ReadDecoded(gameRoot, catalogs, asset);

        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "extracted-assets");
        Directory.CreateDirectory(root);
        var extension = asset.Kind switch
        {
            FrostbiteAssetKind.Ebx => ".ebx",
            FrostbiteAssetKind.Res => ".res",
            _ => ".chunk",
        };
        var safeName = string.IsNullOrWhiteSpace(asset.Sha1)
            ? asset.Name.Replace('-', '_').Replace('/', '_').Replace('\\', '_')
            : asset.Sha1;
        var destination = Path.Combine(root, safeName + extension);
        var temporary = destination + ".tmp";
        File.WriteAllBytes(temporary, payload);
        File.Move(temporary, destination, overwrite: true);
        return destination;
    }

    public static byte[] ReadDecoded(
        string gameRoot,
        IReadOnlyDictionary<uint, string> catalogs,
        FrostbiteIndexedAsset asset)
    {
        var casPath = ResolveCasPath(gameRoot, catalogs, asset);
        var compressed = ReadRange(casPath, asset.Offset, asset.Size);
        var expectedSize = asset.OriginalSize != 0
            ? checked((int)asset.OriginalSize)
            : asset.LogicalSize != 0 ? checked((int)asset.LogicalSize) : 0;
        return Decompress(compressed, expectedSize, gameRoot);
    }

    internal static string ResolveCasPath(
        string root,
        IReadOnlyDictionary<uint, string> catalogs,
        FrostbiteIndexedAsset asset)
    {
        if (!catalogs.TryGetValue(asset.Catalog, out var catalog))
            throw new InvalidDataException($"Unknown catalog 0x{asset.Catalog:X8}.");
        var source = asset.Patch ? "Patch" : "Data";
        var sourceRoot = Path.GetFullPath(Path.Combine(root, source, "Win32"))
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(
            sourceRoot,
            catalog.Replace('/', Path.DirectorySeparatorChar),
            $"cas_{asset.Cas:D2}.cas"));
        if (!path.StartsWith(sourceRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Catalog path escapes the FC26 installation.");
        if (!File.Exists(path)) throw new FileNotFoundException("CAS file is unavailable.", path);
        return path;
    }

    private static byte[] ReadRange(string path, uint offset, uint size)
    {
        if (size == 0 || size > int.MaxValue)
            throw new InvalidDataException($"Invalid compressed asset size: {size}.");
        using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            bufferSize: 128 * 1024, FileOptions.RandomAccess);
        if (offset > stream.Length || size > stream.Length - offset)
            throw new InvalidDataException("Asset payload lies outside its CAS file.");
        var data = new byte[checked((int)size)];
        stream.Position = offset;
        stream.ReadExactly(data);
        return data;
    }

    internal static byte[] Decompress(
        ReadOnlySpan<byte> input, int expectedSize, string gameRoot)
    {
        using var output = expectedSize > 0
            ? new MemoryStream(Math.Min(expectedSize, MaxOutputBytes))
            : new MemoryStream();
        var cursor = 0;
        while (cursor < input.Length)
        {
            if (input.Length - cursor < 8)
                throw new InvalidDataException("Truncated Frostbite codec header.");
            var unpackedWord = BinaryPrimitives.ReadUInt32BigEndian(input.Slice(cursor, 4));
            var packedWord = BinaryPrimitives.ReadUInt32BigEndian(input.Slice(cursor + 4, 4));
            cursor += 8;
            var unpackedSize = checked((int)(unpackedWord & 0x00FFFFFF));
            var methodData = (byte)(unpackedWord >> 24);
            var method = (byte)(packedWord >> 24);
            var guard = (packedWord >> 20) & 0xF;
            var packedSize = checked((int)(packedWord & 0x000FFFFF));
            if (guard != 7) throw new InvalidDataException("Invalid Frostbite codec guard bits.");
            if (unpackedSize < 0 || unpackedSize > MaxOutputBytes ||
                packedSize < 0 || packedSize > input.Length - cursor ||
                output.Length + unpackedSize > MaxOutputBytes)
                throw new InvalidDataException("Frostbite codec block exceeds safety limits.");

            var block = input.Slice(cursor, packedSize);
            cursor += packedSize;
            var decoded = method switch
            {
                0 => DecodeNone(block, unpackedSize),
                2 => DecodeZlib(block, unpackedSize),
                17 or 18 or 21 or 22 or 24 or 25 =>
                    OodleDecoder.Decode(gameRoot, block, unpackedSize),
                15 when methodData != 0 => throw new NotSupportedException(
                    $"Dictionary-compressed ZSTD assets require dictionary {methodData}, which is unavailable."),
                15 => DecodeZstd(block, unpackedSize),
                _ => throw new NotSupportedException(
                    $"Unsupported Frostbite codec method {method}."),
            };
            output.Write(decoded);
        }

        if (expectedSize > 0 && output.Length != expectedSize)
            throw new InvalidDataException(
                $"Asset size mismatch: expected {expectedSize}, decoded {output.Length}.");
        return output.ToArray();
    }

    private static byte[] DecodeNone(ReadOnlySpan<byte> block, int expected)
    {
        if (block.Length != expected)
            throw new InvalidDataException("Uncompressed Frostbite block size mismatch.");
        return block.ToArray();
    }

    private static byte[] DecodeZlib(ReadOnlySpan<byte> block, int expected)
    {
        using var source = new MemoryStream(block.ToArray(), writable: false);
        using var zlib = new ZLibStream(source, CompressionMode.Decompress);
        var result = new byte[expected];
        var total = 0;
        while (total < result.Length)
        {
            var read = zlib.Read(result, total, result.Length - total);
            if (read == 0) break;
            total += read;
        }
        if (total != expected || zlib.ReadByte() != -1)
            throw new InvalidDataException("Zlib Frostbite block size mismatch.");
        return result;
    }

    private static byte[] DecodeZstd(ReadOnlySpan<byte> block, int expected)
    {
        using var decompressor = new Decompressor();
        var result = decompressor.Unwrap(block, expected).ToArray();
        if (result.Length != expected)
            throw new InvalidDataException(
                $"ZSTD Frostbite block size mismatch: expected {expected}, decoded {result.Length}.");
        return result;
    }

    private static class OodleDecoder
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate nuint OodleDecompress(
            IntPtr compressed, nuint compressedSize,
            IntPtr raw, nuint rawSize,
            int fuzzSafe, int checkCrc, int verbosity,
            IntPtr decodeBufferBase, nuint decodeBufferSize,
            IntPtr callback, IntPtr callbackUserData,
            IntPtr decoderMemory, nuint decoderMemorySize,
            int threadPhase);

        public static byte[] Decode(
            string gameRoot, ReadOnlySpan<byte> compressed, int rawSize)
        {
            var libraryPath = Path.Combine(gameRoot, "oo2core_9_win64.dll");
            if (!File.Exists(libraryPath))
                throw new FileNotFoundException(
                    "FC26 Oodle decoder was not found in the game installation.", libraryPath);
            nint library = 0;
            try
            {
                library = NativeLibrary.Load(libraryPath);
                var export = NativeLibrary.GetExport(library, "OodleLZ_Decompress");
                if (export == 0)
                    throw new EntryPointNotFoundException("OodleLZ_Decompress export not found in oo2core_9_win64.dll");
                var decode = Marshal.GetDelegateForFunctionPointer<OodleDecompress>(export);
                var source = compressed.ToArray();
                var destination = new byte[rawSize];
                var sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
                var destinationHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
                try
                {
                    var written = decode(
                        sourceHandle.AddrOfPinnedObject(), checked((nuint)source.Length),
                        destinationHandle.AddrOfPinnedObject(), checked((nuint)destination.Length),
                        0, 0, 0, IntPtr.Zero, 0,
                        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, 3);
                    if (written != checked((nuint)rawSize))
                        throw new InvalidDataException(
                            $"Oodle decoded {written} bytes; expected {rawSize}.");
                    return destination;
                }
                finally
                {
                    destinationHandle.Free();
                    sourceHandle.Free();
                }
            }
            catch (AccessViolationException)
            {
                throw new InvalidOperationException(
                    "Oodle decompression failed — the oo2core_9_win64.dll in the game installation " +
                    "may be corrupted or incompatible. Verify your FC 26 installation.");
            }
            catch (BadImageFormatException)
            {
                throw new InvalidOperationException(
                    "The oo2core_9_win64.dll is not a valid 64-bit library. " +
                    "Verify your FC 26 installation.");
            }
            finally
            {
                if (library != 0) NativeLibrary.Free(library);
            }
        }
    }
}
