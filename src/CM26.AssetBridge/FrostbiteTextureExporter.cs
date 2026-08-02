using System.Buffers.Binary;
using System.Text;

namespace CM26.AssetBridge;

internal static class FrostbiteTextureExporter
{
    public const uint TextureResType = 0x6BDE20BA;

    public static string ExportDds(
        string gameRoot,
        IReadOnlyDictionary<uint, string> catalogs,
        FrostbiteIndexedAsset resource)
    {
        if (resource.Kind != FrostbiteAssetKind.Res || resource.ResType != TextureResType)
            throw new InvalidDataException("Selected RES entry is not a Frostbite texture.");

        var header = FrostbitePayloadReader.ReadDecoded(gameRoot, catalogs, resource);
        if (header.Length < 168)
            throw new InvalidDataException("FC26 texture header is truncated.");

        var pixelFormat = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(12, 4));
        var width = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(22, 2));
        var height = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(24, 2));
        var mipCount = header[30];
        var chunkId = new Guid(header.AsSpan(40, 16));
        var chunk = FrostbiteAssetIndexStore.FindExact(
            chunkId.ToString("D"), FrostbiteAssetKind.Chunk)
            ?? throw new FileNotFoundException($"Texture chunk was not indexed: {chunkId}");
        var pixels = FrostbitePayloadReader.ReadDecoded(gameRoot, catalogs, chunk);

        var ddsHeader = CreateDdsHeader(pixelFormat, width, height, mipCount, pixels.Length);
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "extracted-assets");
        Directory.CreateDirectory(root);
        var destination = Path.Combine(root, resource.Sha1 + ".dds");
        var temporary = destination + ".tmp";
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            stream.Write(ddsHeader);
            stream.Write(pixels);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temporary, destination, overwrite: true);
        return destination;
    }

    private static byte[] CreateDdsHeader(
        int renderFormat, int width, int height, int mipCount, int dataLength)
    {
        var fourCc = renderFormat switch
        {
            54 or 55 or 56 or 57 => "DXT1",
            58 or 59 => "DXT3",
            60 or 61 => "DXT5",
            62 => "ATI1",
            63 => "ATI2",
            _ => throw new NotSupportedException(
                $"FC26 texture RenderFormat {renderFormat} is not supported by the DDS exporter."),
        };

        var header = new byte[128];
        Encoding.ASCII.GetBytes("DDS ").CopyTo(header, 0);
        WriteUInt32(header, 4, 124);
        const uint caps = 0x1;
        const uint heightFlag = 0x2;
        const uint widthFlag = 0x4;
        const uint pixelFormatFlag = 0x1000;
        const uint linearSizeFlag = 0x80000;
        const uint mipMapCountFlag = 0x20000;
        var flags = caps | heightFlag | widthFlag | pixelFormatFlag | linearSizeFlag;
        if (mipCount > 1) flags |= mipMapCountFlag;
        WriteUInt32(header, 8, flags);
        WriteUInt32(header, 12, checked((uint)height));
        WriteUInt32(header, 16, checked((uint)width));
        WriteUInt32(header, 20, checked((uint)dataLength));
        WriteUInt32(header, 28, checked((uint)Math.Max(1, mipCount)));

        WriteUInt32(header, 76, 32);       // DDS_PIXELFORMAT size
        WriteUInt32(header, 80, 0x4);      // DDPF_FOURCC
        Encoding.ASCII.GetBytes(fourCc).CopyTo(header, 84);

        var textureCaps = 0x1000u;         // DDSCAPS_TEXTURE
        if (mipCount > 1) textureCaps |= 0x8u | 0x400000u;
        WriteUInt32(header, 108, textureCaps);
        return header;
    }

    private static void WriteUInt32(byte[] data, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset, 4), value);
}
